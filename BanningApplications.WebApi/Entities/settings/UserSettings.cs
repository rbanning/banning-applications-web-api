using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Entities.settings
{
    public class UserSettings
    {
		[Required, MaxLength(BaseMetaEntityUtil.IdGuidMaxLength)]
		public string UserId { get; set; }
		[Required, MaxLength(BaseMetaEntityUtil.IdGuidMaxLength)]
		public string Scope { get; set; }
		[Required]
		public string Settings { get; set; }

		[Required]
		public DateTime CreateDate { get; set; }
		[Required]
		public DateTime ModifyDate { get; set; }

		public UserSettings()
		{
			this.CreateDate = DateTime.UtcNow;
			this.ModifyDate = DateTime.UtcNow;
		}

	}
}
