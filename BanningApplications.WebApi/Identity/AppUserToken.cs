using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Identity
{
    public class AppUserToken
    {
		public const int TokenStringMaxLength = 50;

		[Required, MaxLength(Entities.BaseMetaEntityUtil.IdStringMaxLength)]
		public string UserId { get; set; }
		[Required, MaxLength(Entities.BaseMetaEntityUtil.IdStringMaxLength)]
		public string Scope { get; set; }
		[Required, MaxLength(Entities.BaseMetaEntityUtil.IdStringMaxLength)]
		public string Type { get; set; }
		[Required, MaxLength(TokenStringMaxLength)]
		public string Token { get; set; }
		[Required]
		public DateTime Expires { get; set; }

		//meta
		[Required]
		public DateTime CreateDate { get; set; }

		public AppUserToken()
		{
			this.CreateDate = DateTime.UtcNow;
		}

		public void SetExpiration(int ttlMinutes)
		{
			this.Expires = DateTime.UtcNow.AddMinutes(ttlMinutes);
		}

		public bool IsExpired()
		{
			return this.Expires < DateTime.UtcNow;
		}
	}
}
