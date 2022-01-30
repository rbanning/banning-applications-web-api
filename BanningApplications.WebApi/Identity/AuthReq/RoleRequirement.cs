using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			ValidRoles = roles.ToList<string>();
		}
	}

}
