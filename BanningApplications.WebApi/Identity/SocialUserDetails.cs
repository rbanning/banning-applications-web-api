using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Identity
{
	public class SocialUserDetails
	{
		public string Email { get; set; }
		public string Name { get; set; }
		public string Locale { get; set; }
		public string Provider { get; set; }
		public string Iss { get; set; }
		public DateTime IssuedAt { get; set; }
		public DateTime ExpiresAt { get; set; }
		public bool IsExpired
		{
			get
			{
				return this.ExpiresAt < DateTime.Now;
			}
		}

		public SocialUserDetails()
		{ }

		public SocialUserDetails(GoogleApiTokenInfo googleApiTokenInfo)
		{
			Provider = "GOOGLE";
			Email = googleApiTokenInfo.email;
			Name = googleApiTokenInfo.name;
			Locale = googleApiTokenInfo.locale;
			Iss = googleApiTokenInfo.iss;
			IssuedAt = googleApiTokenInfo.IssuedAt;
			ExpiresAt = googleApiTokenInfo.ExpiresAt;
		}
	}
}
