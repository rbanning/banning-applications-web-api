using System;
using BanningApplications.WebApi.Helpers;

namespace BanningApplications.WebApi.Identity
{
    public static class MockAuth
    {
	    public const string MOCK_ROLE = "mock";

	    private const string MOCK_KEY =
		    "mockAuthServiceShouldBeUsedWithCautionButAllowsForCreatingQuickAppProofOfConcepts";

	    private const string MOCK_DELIM = "|";
	    
	    
	    public static AppUser CreateAppUser(string email, string scopeId)
	    {
		    var user = new AppUser()
		    {
			    Id = email.ToLowerInvariant().ToBase64().Substring(0, 8),
			    UserName = email.ToLowerInvariant(),
			    Email = email,
			    EmailConfirmed = true,
			    NormalizedEmail = email.ToLowerInvariant(),
			    NormalizedUserName = email.ToLowerInvariant(),
			    Name = "Mock User", //todo: create a random name,
			    PhoneNumber = "111-222-5555"
		    };

		    var scope = RegisteredScopes.Find(scopeId);

		    if (scope != null)
		    {
			    var avatars = new AvatarRepo();
			    var avatarIndex = user.Id.GetHashCode();

			    user.Scope = ObfuscateScopeId(scope);
			    user.Role = "mock";
			    user.Avatar = avatars.GetAvatar(avatarIndex);

			    return user;
		    }

		    //else
		    return null;
	    }

	    private static string ObfuscateScopeId(RegisteredScopes.Scope scope)
	    {
		    if (scope == null)
		    {
			    return null;
		    }
			//else
			try
			{
				return  scope.Secret.Encrypt(ScopeObfuscationKey(scope)) + MOCK_DELIM + scope.Code.Encrypt(MOCK_KEY);

			}
			catch (Exception )
			{
				return null;
			}
	    }

	    private static string ScopeObfuscationKey(RegisteredScopes.Scope scope)
	    {
		    return $"{scope?.Code}{scope?.Contacts.Admin.Email}";
	    }

	    public static string ExtractScopeFromObfuscateScopeId(string obfuscatedScopeId)
	    {
		    if (string.IsNullOrEmpty(obfuscatedScopeId))
		    {
			    return null;
		    }

		    try
		    {
			    var parts = obfuscatedScopeId.Split(MOCK_DELIM);
			    if (parts.Length == 2)
			    {
				    var code = parts[1].Decrypt(MOCK_KEY);
				    var scope = RegisteredScopes.FindByCode(code);
				    if (scope != null)
				    {
					    var secret = parts[0].Decrypt(ScopeObfuscationKey(scope));
					    if (string.Equals(secret, scope.Secret))
					    {
						    return scope.Id;
					    }
				    }
			    }
		    }
		    catch (Exception )
		    {
			    //ignore
		    }

		    return null;
	    }
    }
}
