using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.settings
{
    public class UserSettingsDto
    {
		public string UserId { get; set; }
		public string Scope { get; set; }
		public string Settings { get; set; }
		public DateTime CreateDate { get; set; }
		public DateTime ModifyDate { get; set; }
	}
}
