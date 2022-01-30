using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Identity.AuthReq
{
    public class ScopeRequirement : IAuthorizationRequirement
	{
		public List<string> ValidScopes { get; set; }

		public ScopeRequirement(List<string> validScopes)
		{
			ValidScopes = validScopes;
		}
		public ScopeRequirement(params string[] scopeIds)
		{
			ValidScopes = scopeIds.ToList<string>();
		}
	}

}
