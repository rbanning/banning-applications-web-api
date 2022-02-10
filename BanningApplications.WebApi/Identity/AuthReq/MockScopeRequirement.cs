using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace BanningApplications.WebApi.Identity.AuthReq
{
    public class MockScopeRequirement : IAuthorizationRequirement
    {
	    public List<string> ValidScopes { get; set; }

	    public MockScopeRequirement(List<string> validScopes)
	    {
		    ValidScopes = validScopes;
	    }
	    public MockScopeRequirement(params string[] scopeIds)
	    {
		    ValidScopes = scopeIds.ToList();
	    }
    }
}
