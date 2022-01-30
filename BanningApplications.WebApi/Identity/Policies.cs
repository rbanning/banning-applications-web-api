using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;

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

	}
}
