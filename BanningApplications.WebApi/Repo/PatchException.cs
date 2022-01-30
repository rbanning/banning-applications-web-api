using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Repo
{
    public class PatchException: Exception
    {
		public PatchException(string message)
			:base(message)
		{ }

		public PatchException(string message, Exception innerException)
			:base(message, innerException)
		{ }
    }
}
