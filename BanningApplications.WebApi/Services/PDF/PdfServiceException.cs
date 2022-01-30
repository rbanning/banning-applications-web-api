using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Services.PDF
{
    public class PdfServiceException: Exception
    {
	    public PdfServiceException(string message)
			:base(message)
	    { }

	    public PdfServiceException(string message, Exception innerException)
			:base(message, innerException)
	    { }
    }
}
