using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Services.WorldTime
{
    public class WorldTimeException: Exception
    {
	    public WorldTimeException(string message)
			:base(message) { }

	    public WorldTimeException(string message, Exception innerException)
			:base(message, innerException) { }

    }
}
