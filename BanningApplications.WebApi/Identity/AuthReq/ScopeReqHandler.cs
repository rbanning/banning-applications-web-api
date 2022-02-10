using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Identity.AuthReq
{
	public class ScopeReqHandler : AuthorizationHandler<ScopeRequirement>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
		{
			var user = context.User;
			string scope = user.FindClaimValue(RegisteredClaimNames.Scope);

			if (requirement.ValidScopes.Any(r => string.Equals(r, scope, StringComparison.CurrentCultureIgnoreCase)))
			{
				context.Succeed(requirement);
			}

			return Task.CompletedTask;
		}
	}

}
