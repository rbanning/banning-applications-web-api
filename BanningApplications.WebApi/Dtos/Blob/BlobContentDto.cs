using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.Blob
{
    public class BlobContentDto
    {
		[Required, MaxLength(100)]
		public string Filename { get; set; }
		[Required, MaxLength(Services.Blob.BlobService.MAX_CONTENT_LENGTH)]
		public string Content { get; set; }

		public bool IsValid()
		{
			return !string.IsNullOrEmpty(Content)
				&& !string.IsNullOrEmpty(Filename)
				&& Filename.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0;
		}

	}
}
