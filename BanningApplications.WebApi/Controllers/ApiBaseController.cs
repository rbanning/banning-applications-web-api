using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using BanningApplications.WebApi.Identity;
using Microsoft.Extensions.Primitives;

// ReSharper disable RedundantNameQualifier

namespace BanningApplications.WebApi.Controllers
{
	public class ApiBaseController: ControllerBase
	{
		protected Identity.AppUser GetAppUser()
		{
			if (User != null)
			{
				try
				{					
					var ret = User.ToAppUser();
					//check for mock user
					if (string.Equals(ret.Role, MockAuth.MOCK_ROLE))
					{
						ret.Scope = MockAuth.ExtractScopeFromObfuscateScopeId(ret.Scope);
					}

					return ret;
				}
				catch (Exception)
				{
					//DEBUG: GetAppUser
					Console.WriteLine("Error getting app user from auth token");
					throw;
				}
			}

			//else
			return null;
		}

		protected string GetAppUserId()
		{
			var user = GetAppUser();
			return user == null || string.IsNullOrEmpty(user.Id) ? null : user.Id;
		}

		protected (Identity.AppUser appUser, Identity.RegisteredScopes.Scope scope) GetUserAndScope()
		{
			var appUser = GetAppUser();
			if (appUser != null)
			{
				var scope = Identity.RegisteredScopes.Find(appUser.Scope);
				return (appUser, scope);
			}

			//else
			return (null, null);
		}

		#region >> Dynamic Identity <<

		protected Identity.RegisteredScopes.Scope.DynamicIdentity GetDynamicIdentity()
		{
			//header values
			var values = DynamicIdentityHeaders();
			if (values.Any(pair => string.IsNullOrEmpty(pair.Value)))
			{
				//todo: log the error?  throw an error?
				return null;
			}

			//get scope and verify scope code
			var scope = Identity.RegisteredScopes.Find(values["SCOPE"]);
			if (scope == null || !string.Equals(scope.Code, values["SCOPE-CODE"], StringComparison.CurrentCultureIgnoreCase))
			{
				//todo: log the error? throw an error?
				return null;
			}

			return new RegisteredScopes.Scope.DynamicIdentity(scope, values["USER"], values["CHALLENGE"],
				values["CODE"]);

		}

		protected bool IsDynamicIdentityManager(Identity.RegisteredScopes.Scope.DynamicIdentity identity)
		{
			if (identity == null || !identity.IsValid)
			{
				return false;
			}

			//get the manager contact header
			var manager = Request.Headers.FirstOrDefault(m => string.Equals(m.Key,"X-Hallpass-DynamicIdentity-Contact", StringComparison.CurrentCultureIgnoreCase)).Value;

			//todo: make this a bit more secure?
			//contact should equal the user 
			return !(StringValues.IsNullOrEmpty(manager)) && string.Equals(identity.User, manager.First());
		}


		protected Dictionary<string, string> DynamicIdentityHeaders()
		{
			return new Dictionary<string, string>
			{
				{ "SCOPE", Request.Headers.FirstOrDefault(m => string.Equals(m.Key,"X-Hallpass-Scope", StringComparison.CurrentCultureIgnoreCase)).Value },
				{ "SCOPE-CODE", Request.Headers.FirstOrDefault(m => string.Equals(m.Key, "X-Hallpass-Scope-Code", StringComparison.CurrentCultureIgnoreCase)).Value },
				{ "CHALLENGE", Request.Headers.FirstOrDefault(m => string.Equals(m.Key, "X-Hallpass-DynamicIdentity-Challenge", StringComparison.CurrentCultureIgnoreCase)).Value },
				{ "CODE", Request.Headers.FirstOrDefault(m => string.Equals(m.Key, "X-Hallpass-DynamicIdentity-Code", StringComparison.CurrentCultureIgnoreCase)).Value },
				{ "USER", Request.Headers.FirstOrDefault(m => string.Equals(m.Key, "X-Hallpass-DynamicIdentity-User", StringComparison.CurrentCultureIgnoreCase)).Value }
			};
		}


		//NOTE WebhookDynamicIdentity does not use DynamicIdentity validation!!!

		protected Identity.RegisteredScopes.Scope.DynamicIdentity GetWebhookDynamicIdentity(string authKey)
		{
			//WARNING: this by-passes the DynamicIdentity validation
			return GetWebhookDynamicIdentity(
				authKey,
				Request.Query.FirstOrDefault(m => m.Key == "auth").Value.ToString(),
				Request.Query.FirstOrDefault(m => m.Key == "scope").Value.ToString(),
				Request.Query.FirstOrDefault(m => m.Key == "code").Value.ToString(),
				Request.Query.FirstOrDefault(m => m.Key == "usr").Value.ToString()
			);
		}

		protected Identity.RegisteredScopes.Scope.DynamicIdentity GetWebhookDynamicIdentity(
			string authKey,
			string authValue,  //authValue must be contained within authKey (lowercase of each)
			string scopeId,
			string scopeCode,
			string usr)
		{
			if (authKey == null || authValue == null)
			{
				return null;
			}

			authKey = authKey.ToLower();
			authValue = authValue.ToLower();

			//WARNING: this by-passes the DynamicIdentity validation
			var identity = new Identity.RegisteredScopes.Scope.DynamicIdentity()
			{
				Scope = Identity.RegisteredScopes.Find(scopeId)
			};

			if (identity.Scope?.Code == scopeCode && authKey.Contains(authValue))
			{
				identity.User = usr;
			}

			return identity;
		}
		#endregion


	}
}
