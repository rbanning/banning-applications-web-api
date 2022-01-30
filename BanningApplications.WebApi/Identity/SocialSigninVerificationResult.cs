using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Identity
{
	public class SocialSigninVerificationResult
	{
		public SocialUserDetails UserInfo { get; set; }
		public HttpStatusCode StatusCode { get; set; }
		public string Reason { get; set; }

		public bool IsValid
		{
			get
			{
				return UserInfo != null;
			}
		}
		public SocialSigninVerificationResult(HttpStatusCode statusCode)
		{
			this.StatusCode = statusCode;
		}

		public SocialSigninVerificationResult(HttpStatusCode statusCode, SocialUserDetails userInfo)
		{
			UserInfo = userInfo;
			StatusCode = statusCode;
		}

		public SocialSigninVerificationResult(SocialUserDetails userInfo)
			:this(HttpStatusCode.OK, userInfo)
		{ }

		public SocialSigninVerificationResult(HttpStatusCode statusCode, string reason)
		{
			this.StatusCode = statusCode;
			this.Reason = reason;
		}


	}
}
