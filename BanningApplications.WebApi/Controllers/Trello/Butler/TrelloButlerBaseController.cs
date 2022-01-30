using System;
using System.Linq;
using BanningApplications.WebApi.Services.Trello;

namespace BanningApplications.WebApi.Controllers.Trello.Butler
{
    public class TrelloButlerBaseController: ApiBaseController
    {
		// ReSharper disable once InconsistentNaming
		protected readonly ITrelloService _trelloService;

		public TrelloButlerBaseController(ITrelloService trelloService)
		{
			_trelloService = trelloService;
		}


		#region >> Auto-Configure TrelloService via DynamicIdentity <<

		//Uses 'scope' query param (or header) to create DynamicIdentity
		//		scope = {code}-{secret}-{id start}-{user}

		protected (Identity.RegisteredScopes.Scope scope, string user) GetIdentityFromRequest()
		{
			//query values
			var authCode = Request.Query.FirstOrDefault(m => string.Equals(m.Key, "scope", StringComparison.CurrentCultureIgnoreCase));

			if (authCode.Value.Count != 1)
			{
				//see if the authCode is in the header
				authCode = Request.Headers.FirstOrDefault(m => string.Equals(m.Key, "x-scope", StringComparison.CurrentCultureIgnoreCase));
				if (authCode.Value.Count != 1)
				{
					return (null, null);
				}
			}

			if (authCode.Value.Count != 1)
			{
				return (null, null);
			}

			var values = authCode.Value.First().Split("-");
			if (values.Length != 4)
			{
				return (null, null);
			}

			//get scope and verify scope code
			var scope = Identity.RegisteredScopes.FindByCode(values[0]);
			if (scope == null ||
			    !string.Equals(scope.Secret, values[1], StringComparison.CurrentCultureIgnoreCase) ||
			    !scope.Id.StartsWith(values[2]))
			{
				return (null, null);
			}

			var user = values[3];

			return (scope, user);
		}
		protected bool AutoConfigureTrelloServiceIfNeeded()
		{
			if (!_trelloService.IsConfigured)
			{
				var identity = GetIdentityFromRequest();
				if (identity.scope == null || identity.user == null) { return false; }

				var config = TrelloConfig.Find(identity.scope);
				if (config == null) { return false; }

				_trelloService.Configure(config);
			}

			return true;
		}

		#endregion
    }

}
