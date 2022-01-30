using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.ValueConverters
{
    public static class Constants
    {
		public static string LIST_DELIM
		{
			get
			{
				return Helpers.StringListExtensions.STRING_DELIM;
			}
		}
	}
}
