using BanningApplications.WebApi.Dtos.Blob;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;

namespace BanningApplications.WebApi.Services.Blob
{
	public interface IBlobService
	{
		int MaxContentLength { get; }
		List<string> AcceptedContentTypes { get; }
		List<string> AcceptedExtensions { get; }
		string ExtensionToContentType(string extension);
		string ContentTypeToExtension(string contentType);

		Task<bool> BlobExistsAsync(string containerName, string name);
		Task<BlobDto> GetInfoAsync(string containerName, string name);
		Task<BlobDownloadInfo> GetBlobDownloadInfoAsync(string containerName, string name);

		Task<IEnumerable<BlobDto>> ListBlobsAsync(string containerName, string folder = null);
		Task<List<BlobItem>> ListBlobsAsItemAsync(string containerName, string folder = null);

		Task<BlobDto> UploadBase64ToBlobAsync(string containerName, string sourceBase64, string contentType, string filename);
		Task<BlobDto> UploadFileToBlobAsync(string containerName, string sourceFilename, string contentType, string filename = null);
		Task<BlobDto> UploadFileToBlobAsync(string containerName, Microsoft.AspNetCore.Http.IFormFile file, string filename = null);

		//Task DeleteBlobAsync(string containerName, string name);
	}

}
