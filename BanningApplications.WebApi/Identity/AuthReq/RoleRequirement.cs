using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;

namespace BanningApplications.WebApi.Identity.AuthReq
{
	public class RoleRequirement : IAuthorizationRequirement
	{
		public List<string> ValidRoles { get; set; }

		public RoleRequirement(List<string> validRoles)
		{
			ValidRoles = validRoles;
		}
		public RoleRequirement(params string[] roles)
		{
			ValidRoles = roles.ToList();
		}
	}

}
