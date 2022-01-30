using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Services.Blob
{
    public class BlobServiceException: Exception
    {
		public BlobServiceException(string message)
			:base(message)
		{ }

		public BlobServiceException(string message, Exception innerException)
			:base(message, innerException)
		{ }
    }
}
