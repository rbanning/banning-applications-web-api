using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Identity.AuthReq
{
	public class RoleReqHandler : AuthorizationHandler<RoleRequirement>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RoleRequirement requirement)
		{
			var user = context.User;
			string role = user.FindClaimValue(RegisteredClaimNames.Role);

			if (requirement.ValidRoles.Any(r => string.Equals(r, role, StringComparison.CurrentCultureIgnoreCase)))
			{
				context.Succeed(requirement);
			}

			return Task.CompletedTask;
		}
	}

}
