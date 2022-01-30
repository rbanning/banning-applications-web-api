using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BanningApplications.WebApi.Dtos.Blob
{
	public class BlobDto
	{
		public string Name { get; set; }
		public string Uri { get; set; }
		public long ContentLength { get; set; }
		public string ContentType { get; set; }

		public BlobDto()
		{
			ContentLength = -1;
		}

		public BlobDto(BlobClient blob, BlobProperties properties = null)
			:this()
		{
			Name = blob.Name;
			Uri = blob.Uri?.AbsoluteUri;
			if (properties != null)
			{
				ContentLength = properties.ContentLength;
				ContentType = properties.ContentType;
			}
		}

		public BlobDto(BlobItem blob)
			:this()
		{
			Name = blob.Name;
			if (blob.Properties != null)
			{
				ContentLength = blob.Properties.ContentLength ?? -1;
				ContentType = blob.Properties.ContentType;
			}
		}

	}
}
