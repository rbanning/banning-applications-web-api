using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
// ReSharper disable InconsistentNaming

namespace BanningApplications.WebApi.Identity
{
	public static class Policies
	{
		public static class Names
		{
			//jwt (api)
			public const string AllowRoot = "AllowRoot";
			public const string AllowAdmin = "AllowAdmin";
			public const string AllowManager = "AllowManager";
			public const string AllowCustomer = "AllowCustomer";
			public const string AllowViewer = "AllowViewer";
		}

		//See AuthReq.RoleRequirement and associated AuthReq.RoleReqHandler (as defined in AuthHelper.ConfigureAuthorization())
		public static AuthorizationPolicyBuilder AllowRoles(params string[] roles)
		{
			return new AuthorizationPolicyBuilder()
			  .AddAuthenticationSchemes("Bearer")
			  .RequireAuthenticatedUser()
			  .RequireAssertion(ctx =>
			  {
				  if (ctx.User.HasClaim(m => m.Type == RegisteredClaimNames.Role))
				  {
					  return roles.Contains(ctx.User.FindFirstValue(RegisteredClaimNames.Role));
				  }
					  //else
					  return false;
			  });
		}

		//See AuthReq.ScopeRequirement and associated AuthReq.ScopeReqHandler (as defined in AuthHelper.ConfigureAuthorization())
		public static AuthorizationPolicyBuilder AllowScope(params string[] scopes)
		{
			return new AuthorizationPolicyBuilder()
			  .AddAuthenticationSchemes("Bearer")
			  .RequireAuthenticatedUser()
			  .RequireAssertion(ctx =>
			  {
				  if (ctx.User.HasClaim(m => m.Type == RegisteredClaimNames.Scope))
				  {
					  return scopes.Contains(ctx.User.FindFirstValue(RegisteredClaimNames.Scope));
				  }
				  //else
				  return false;
			  });
		}

		//See AuthReq.MockScopeRequirement and associated AuthReq.MockScopeReqHandler (as defined in AuthHelper.ConfigureAuthorization())
		public static AuthorizationPolicyBuilder AllowMockScope(params string[] scopes)
		{
			return new AuthorizationPolicyBuilder()
			  .AddAuthenticationSchemes("Bearer")
			  .RequireAuthenticatedUser()
			  .RequireAssertion(ctx =>
			  {
				  if (ctx.User.HasClaim(m => m.Type == RegisteredClaimNames.Scope))
				  {
					  var scope = ctx.User.FindFirstValue(RegisteredClaimNames.Scope);
					  try
					  {
						  return scopes.Contains(scope) || scopes.Contains(MockAuth.ExtractScopeFromObfuscateScopeId(scope));

					  }
					  catch (Exception)
					  {
						  //ignore
					  }
				  }
				  //else
				  return false;
			  });
		}

	}
}
