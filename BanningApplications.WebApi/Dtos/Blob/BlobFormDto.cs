using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.Blob
{
    public class BlobFormDto
    {
		[Required]
		public Microsoft.AspNetCore.Http.IFormFile File { get; set; }
		[MaxLength(100)]
		public string Filename { get; set; }

		public bool IsValid()
		{
			return File != null && File.Length > 0
				&& !string.IsNullOrEmpty(Filename)
				&& Filename.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) < 0;
		}
	}
}
