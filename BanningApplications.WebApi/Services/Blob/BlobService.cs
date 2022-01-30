using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BanningApplications.WebApi.Dtos.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BanningApplications.WebApi.Services.File;

namespace BanningApplications.WebApi.Services.Blob
{
	public class BlobService : IBlobService
	{
		// 1 MB = 1048576;
		// Set the limit to 3 MB = 3 * 1048576;
		public const int MAX_CONTENT_LENGTH = 3 * 1048576;
		private readonly BlobServiceClient _blobServiceClient;

		public BlobService(BlobServiceClient blobServiceClient)
		{
			_blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));

			AcceptedContentTypes = FileService.AcceptedContentTypes;
			AcceptedExtensions = FileService.AcceptedExtensions;
		}

		public int MaxContentLength => MAX_CONTENT_LENGTH;

		//alias - original moved to FileService
		public string ExtensionToContentType(string extension)
		{
			return FileService.ExtensionToContentType(extension);
		}

		public string ContentTypeToExtension(string contentType)
		{
			return FileService.ContentTypeToExtension(contentType);
		}

		public List<string> AcceptedContentTypes { get; }
		public List<string> AcceptedExtensions { get; }


		public async Task<BlobDownloadInfo> GetBlobDownloadInfoAsync(string containerName, string name)
		{
			if (string.IsNullOrEmpty(containerName)) { throw new ArgumentNullException(nameof(containerName)); }

			var containerClient = await GetOrCreateBlobContainerClientAsync(containerName);
			var blobClient = containerClient.GetBlobClient(name);

			var response = await blobClient.DownloadAsync();
			return response.Value;
		}

		public async Task<BlobDto> GetInfoAsync(string containerName, string name)
		{
			if (string.IsNullOrEmpty(containerName)) { throw new ArgumentNullException(nameof(containerName)); }

			var containerClient = await GetOrCreateBlobContainerClientAsync(containerName);

			var blobClient = containerClient.GetBlobClient(name);
			BlobProperties properties = await blobClient.GetPropertiesAsync();

			return new BlobDto(blobClient, properties);
		}

		public async Task<bool> BlobExistsAsync(string containerName, string name)
		{
			if (string.IsNullOrEmpty(containerName)) { throw new ArgumentNullException(nameof(containerName)); }

			var containerClient = await GetOrCreateBlobContainerClientAsync(containerName);

			var blobClient = containerClient.GetBlobClient(name);
			return await blobClient.ExistsAsync();
		}

		public async Task<IEnumerable<BlobDto>> ListBlobsAsync(string containerName, string folder = null)
		{
			if (string.IsNullOrEmpty(containerName)) { throw new ArgumentNullException(nameof(containerName)); }

			var containerClient = await GetOrCreateBlobContainerClientAsync(containerName);

			var enumerator = containerClient.GetBlobsAsync(prefix: folder).GetAsyncEnumerator();
			try
			{
				var results = new List<BlobDto>();
				while (await enumerator.MoveNextAsync())
				{
					results.Add(new BlobDto(enumerator.Current));
				}

				return results;
			}
			finally
			{
				await enumerator.DisposeAsync();
			}
		}

		public async Task<List<BlobItem>> ListBlobsAsItemAsync(string containerName, string folder = null)
		{
			if (string.IsNullOrEmpty(containerName)) { throw new ArgumentNullException(nameof(containerName)); }

			var containerClient = await GetOrCreateBlobContainerClientAsync(containerName);

			var enumerator = containerClient.GetBlobsAsync(prefix: folder).GetAsyncEnumerator();
			try
			{
				var results = new List<BlobItem>();
				while (await enumerator.MoveNextAsync())
				{
					results.Add(enumerator.Current);
				}

				return results;
			}
			finally
			{
				await enumerator.DisposeAsync();
			}
		}


		public async Task<BlobDto> UploadBase64ToBlobAsync(string containerName, string sourceBase64, string contentType, string filename)
		{
			if (string.IsNullOrEmpty(containerName)) { throw new ArgumentNullException(nameof(containerName)); }
			if (string.IsNullOrEmpty(sourceBase64)) { throw new BlobServiceException("Invalid source: base64 data is empty"); }
			if (!AcceptedContentTypes.Any(m => string.Equals(m, contentType, StringComparison.CurrentCultureIgnoreCase))) { throw new BlobServiceException("Content type is not supported"); }

			var sourceBytes = Convert.FromBase64String(sourceBase64);

			var containerClient = await GetOrCreateBlobContainerClientAsync(containerName);

			var blobClient = containerClient.GetBlobClient(filename);
			var header = new BlobHttpHeaders()
			{
				ContentType = contentType
			};

			await blobClient.UploadAsync(new MemoryStream(sourceBytes), true);
			await blobClient.SetHttpHeadersAsync(header);
			BlobProperties properties = await blobClient.GetPropertiesAsync();

			return new BlobDto(blobClient, properties);
		}

		public async Task<BlobDto> UploadFileToBlobAsync(string containerName, string sourceFilename, string contentType, string filename = null)
		{
			if (string.IsNullOrEmpty(containerName)) { throw new ArgumentNullException(nameof(containerName)); }
			if (string.IsNullOrEmpty(sourceFilename)) { throw new BlobServiceException("Invalid source filename: name is empty"); }
			if (!System.IO.File.Exists(sourceFilename)) { throw new BlobServiceException("Invalid source filename: file is missing"); }
			if (!AcceptedContentTypes.Any(m => string.Equals(m, contentType, StringComparison.CurrentCultureIgnoreCase))) { throw new BlobServiceException("Content type is not supported"); }

			var containerClient = await GetOrCreateBlobContainerClientAsync(containerName);

			filename ??= GenerateFileName(sourceFilename);
			var blobClient = containerClient.GetBlobClient(filename);
			var header = new BlobHttpHeaders()
			{
				ContentType = contentType
			};

			await using (FileStream file = System.IO.File.OpenRead(sourceFilename))
			{
				await blobClient.UploadAsync(file, true);	//replace existing
			}

			await blobClient.SetHttpHeadersAsync(header);
			BlobProperties properties = await blobClient.GetPropertiesAsync();

			return new BlobDto(blobClient, properties);
		}

		public async Task<BlobDto> UploadFileToBlobAsync(string containerName, Microsoft.AspNetCore.Http.IFormFile file, string filename = null)
		{
			if (string.IsNullOrEmpty(containerName)) { throw new ArgumentNullException(nameof(containerName)); }
			if (file == null) { throw new BlobServiceException("Missing file: file is null"); }
			if (file.Length <= 0) { throw new BlobServiceException("File is empty: length = 0"); }
			if (!AcceptedContentTypes.Any(m => string.Equals(m, file.ContentType, StringComparison.CurrentCultureIgnoreCase))) { throw new BlobServiceException("Content type is not supported"); }


			var containerClient = await GetOrCreateBlobContainerClientAsync(containerName);

			filename ??= GenerateFileName(file.FileName);
			var blobClient = containerClient.GetBlobClient(filename);
			var header = new BlobHttpHeaders()
			{
				ContentType = file.ContentType
			};

			await using (var stream = file.OpenReadStream())
			{
				await blobClient.UploadAsync(stream, true);	//replace existing
			}

			await blobClient.SetHttpHeadersAsync(header);
			BlobProperties properties = await blobClient.GetPropertiesAsync();

			return new BlobDto(blobClient, properties);
		}


		#region >> HELPERS <<

		private string GenerateFileName(string filename)
		{
			string[] strName = filename.Split('.');
			return DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd") + "." + DateTime.Now.ToUniversalTime().ToString("yyyyMMdd\\THHmmssfff") + "." + strName[^1]; //using from end expression (index operator) ~ stringName[strName.Length - 1]
		}

		private async Task<BlobContainerClient> GetOrCreateBlobContainerClientAsync(string containerName)
		{			
			var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
			bool exists = await containerClient.ExistsAsync();
			if (!exists) {
				//create container with public access only to the blobs (not the container metadata or list of blobs in the container
				containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName, PublicAccessType.Blob);
			}

			return containerClient;
		}

		#endregion
	}
}
