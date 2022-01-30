using System;
using BanningApplications.WebApi.Identity;

namespace BanningApplications.WebApi.Services.Trello
{
    public class TrelloConfig
    {
	    public string ApiKey { get; private set; }  
	    public string Token { get; private set; }
		public string OrganizationId { get; private set; }

	    public bool IsValid => !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(Token);

	    public TrelloConfig(string apiKey, string token, string ordId)
	    {
		    ApiKey = apiKey;
		    Token = token;
		    OrganizationId = ordId;
	    }


		//todo: move these (key/token) to secrets
	    public static TrelloConfig Trg()
	    {
		    return new TrelloConfig("14374cda2ef7dc8cc5d2a4dfde2b7c0d", "f32bd0c1e921f952271cc97a2d7a1c4aa666bb6883653db509d78d1566ae32ae", "5934d364e6af2d9a233d77b7");
	    }

	    public static TrelloConfig Hallpass()
	    {
		    return new TrelloConfig("fa4a78dc8c9755e1d3ac9fa0c41f9500", "fdd7e3470156bf98578902c402311f9fe96cb02668b69a8455024fbcedd55df3", "5e0e1eec0b88a38d8ce6d989");
	    }

	    public static TrelloConfig Dev()
	    {
		    return new TrelloConfig("b3c9623981863fcf65be727726abcd84", "a63263e3315ac16f622fc66cf04a070a724873bd4c471693f25a5cd87cf9f406", "613137c2fe7bbb58bf630c0f");
	    }

	    public static TrelloConfig Find(RegisteredScopes.Scope scope, bool throwExceptionOnNull = false)
	    {
		    if (scope == null)
		    {
			    if (throwExceptionOnNull)
			    {
				    throw new ArgumentNullException(nameof(scope));
				}
				//else
				return null;
		    }

		    switch (scope.Code.ToLower())
		    {
				case "trg":
					return Trg();
				case "hallpass":
					return Hallpass();
				case "dev":
					return Dev();
				default:
					return null;
		    }
	    }

	    public static bool Exists(RegisteredScopes.Scope scope)
	    {
		    return Find(scope) != null;
	    }
    }
}
