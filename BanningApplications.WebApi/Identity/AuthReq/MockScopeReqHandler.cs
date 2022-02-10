using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Identity.AuthReq
{
    public class MockScopeReqHandler : AuthorizationHandler<MockScopeRequirement>
    {
	    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MockScopeRequirement requirement)
	    {

			var user = context.User;
			string scope = user.FindClaimValue(RegisteredClaimNames.Scope);

			//first check if the scope is a regular (none mock/obfuscated) scope id
			if (IsValidScopeId(scope, requirement))
			{
				context.Succeed(requirement);
			}
			else
			{
				//try to extract the scope id from the value in the auth claim
				try
				{
					scope = MockAuth.ExtractScopeFromObfuscateScopeId(scope);
					if (IsValidScopeId(scope, requirement))
					{
						context.Succeed(requirement);
					}
				}
				catch (Exception)
				{
					//ignore 
				}
			}

			return Task.CompletedTask;
	    }

	    private bool IsValidScopeId(string scope, MockScopeRequirement requirement)
	    {
		    return requirement.ValidScopes.Any(r => string.Equals(r, scope, StringComparison.CurrentCultureIgnoreCase));
	    }
    }
}
