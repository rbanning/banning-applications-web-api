using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BanningApplications.WebApi.Helpers;

namespace BanningApplications.WebApi.Identity
{
	public class SocialSigninValidator
	{
		private IHttpClientFactory _clientFactory;

		public SocialSigninValidator(IHttpClientFactory clientFactory) 
		{
			_clientFactory = clientFactory;
		}

		public async Task<SocialSigninVerificationResult> VerifyGoogleSignin(string idToken, List<string> validClientIds)
		{
			var request = new HttpRequestMessage(HttpMethod.Get,
				$"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={idToken}");

			var client = _clientFactory.CreateClient("google");

			var response = await client.SendAsync(request);

			var ret = new SocialSigninVerificationResult(response.StatusCode, response.ReasonPhrase);

			if (response.IsSuccessStatusCode)
			{
				try
				{
					var token = await response.Content
						.ReadAsAsync<GoogleApiTokenInfo>();
					if (token == null)
					{
						ret.Reason = "No user information was parsed from token";
					} else if (!validClientIds.Contains(token.aud))
					{
						ret.Reason = "Unable to verify token - invalid 'aud' field";
					} else
					{
						ret.UserInfo = new SocialUserDetails(token);
					}
				}
				catch (Exception ex)
				{
					ret.Reason = ex.Message;
				}
			}

			return ret;
		}
	}
}
