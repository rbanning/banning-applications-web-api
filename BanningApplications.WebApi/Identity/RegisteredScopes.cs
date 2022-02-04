using System;
using System.Collections.Generic;
using System.Linq;
using BanningApplications.WebApi.Services.Trello;
using System.Text.Json.Serialization;
using BanningApplications.WebApi.Services.Slack;

// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantCast

namespace BanningApplications.WebApi.Identity
{
	public static class RegisteredScopes
	{
		private static List<Scope> _scopes;

		static RegisteredScopes()
		{
			_scopes = new List<Scope>()
			{
				new Scope() { Id = "ad6f0537-a813-4fbc-9fea-a5c3572123a6", Code = "sandbox", Name = "Sandbox", Secret = "0xBG9", TrelloConfig = TrelloConfig.Dev(), 
					Contacts = RegisteredContacts.Get(RegisteredContacts.ContactGroup.BanningApps) },

				new Scope() { Id = "c20d8ccd-fe69-41a3-8936-7d61a2b848e2", Code = "hallpass", Name = "Hallpass and Friends", Secret = "1xQQ4", TrelloConfig = TrelloConfig.Hallpass(), 
					Contacts = RegisteredContacts.Get(RegisteredContacts.ContactGroup.Hallpass), SlackConfig = SlackConfig.Hallpass() },

				new Scope() { Id = "7e5295ae-ee02-4a14-a09a-42248678c608", Code = "loteria", Name = "BannApps Lotería", Secret = "2xXQ3", RequireEmailValidation = true,
					Contacts = RegisteredContacts.Get(RegisteredContacts.ContactGroup.BanningApps) },

				new Scope() { Id = "e4bf3789-1e8b-4dc4-8210-444237ba3ebe", Code = "bannapps", Name = "Banning Application", Secret = "3xPR7", TrelloConfig = TrelloConfig.Hallpass(), 
					Contacts = RegisteredContacts.Get(RegisteredContacts.ContactGroup.BanningApps), SlackConfig = SlackConfig.Hallpass() },

			};
		}

		public static bool IsValid(string id)
		{
			return _scopes.Any(m => string.Equals(m.Id, id, StringComparison.CurrentCultureIgnoreCase));
		}
		public static Scope Find(string id)
		{
			return _scopes.FirstOrDefault(m => string.Equals(m.Id, id, StringComparison.CurrentCultureIgnoreCase));
		}
		public static Scope FindByCode(string code)
		{
			return _scopes.FirstOrDefault(m => string.Equals(m.Code, code, StringComparison.CurrentCultureIgnoreCase));
		}
		public static string GetId(string code)
		{
			var scope = _scopes.FirstOrDefault(m => string.Equals(m.Code, code, StringComparison.CurrentCultureIgnoreCase));
			return scope == null ? "??invalid??" : scope.Id;
		}
		public static List<Scope> All()
		{
			return _scopes;
		}

		public class Scope
		{
			public string Id { get; set; }
			public string Code { get; set; }
			public string Name { get; set; }
			public string Secret { get; set; }

			//OPTIONAL Access to a Trello config pair
			[JsonIgnore]
			public Services.Trello.TrelloConfig TrelloConfig { get; set; }

			//OPTIONAL Access to Slack
			[JsonIgnore] public Services.Slack.SlackConfig SlackConfig { get; set; }

			public RegisteredContacts.ContactSet Contacts { get; set; }
			[JsonIgnore]
			public List<string> SupportedGoogleClientIds { get; set; }

			public bool RequireEmailValidation { get; set; }
			public bool AllowAutoRegisterUser { get; set; }

			public bool AllowGoogleSignin => SupportedGoogleClientIds != null && SupportedGoogleClientIds.Count > 0;

			public Scope()
			{
				this.SupportedGoogleClientIds = new List<string>();
				this.RequireEmailValidation = true;
				this.AllowAutoRegisterUser = false;
			}

			public bool IsEqualTo(string scopeId)
			{
				return string.Equals(Id, scopeId, StringComparison.CurrentCultureIgnoreCase);
			}

			#region DynamicIdentity

			public class DynamicIdentity
			{
				public Scope Scope { get; set; }
				public string User { get; set; }

				public bool IsValid => Scope != null && !string.IsNullOrEmpty((User));

				public DynamicIdentity()
				{ }

				public DynamicIdentity(Scope scope, string user, string challenge, string code)
				{
					Load(scope, user, challenge, code);
				}

				public bool Load(Scope scope, string user, string challenge, string code)
				{
					//reset
					Scope = null;
					User = null;

					if (scope == null)
					{
						throw new ArgumentNullException(nameof(scope));
					}

					if (scope.IsDynamicIdentityValid(user, challenge, code))
					{
						Scope = scope;
						User = user;
					}

					return IsValid;
				}

			}

			/*** DynamicIdentity ***
			 *
			 * DynamicIdentity is used for lightweight authentication and used when token authentication
			 * is not possible.  Example Uses:
			 *		from Trello Popup Ups
			 *
			 * WARNING - only use DynamicIdentity on applications where the users are authenticated
			 *			by some other means.
			 */


			public bool IsDynamicIdentityValid(string user, string challenge, string code)
			{
				return string.Equals(code, DynamicIdentityCode(user, challenge));
			}

			/// <summary>
			/// DynamicIdentityCode is used for lightweight authentication tied to a scope
			/// Users submit a scope, challenge and identity code.  From the first two, we can generate the DynamicIdentityCode and compare it with the user's.
			/// The challenge must be configured property for the DynamicIdentityCode to work.
			/// See notes in keep.google.com 
			/// </summary>
			/// <param name="user">Identity User</param>
			/// <param name="challenge">Identity Challenge</param>
			/// <returns>string</returns>
			///
			public string DynamicIdentityCode(string user, string challenge)
			{
				//bypass (for local testing)
				if (string.Equals(user, "rob@myhallpass.com") && string.Equals(challenge, "malcolm"))
				{
					return "chester";
				}

				if (!ValidateDynamicIdentityChallenge(challenge, user))
				{
					return null;
				}

				var parts = challenge.Split('-');
				var scope = Id.ToLower();
				var code = "";

				code += scope[parts[1].Length % scope.Length];		//1
				code += scope[challenge.Length % scope.Length];		//2
				code += scope[parts[0].Length % scope.Length];		//3
				code += scope[user.Length % scope.Length];		//4
				code += scope[parts[2].Length % scope.Length];		//5
				code += scope[(int)challenge[0] % scope.Length];	//6

				return code;
			}

			protected bool ValidateDynamicIdentityChallenge(string challenge, string user)
			{
				if (string.IsNullOrEmpty(challenge) || string.IsNullOrEmpty(user))
				{
					return false;
				}

				user = user.ToLower();

				var parts = challenge.Split('-');
				if (parts.Length != 3)
				{
					return false;
				}

				//assert: Both User and Code must be at least 3 chars long
				if (Code.Length < 3)
				{
					throw new Exception("Code must be at least three chars long");

				}

				//generate DOW
				var date = DateTime.UtcNow;
				var dow = date.ToString("D").Substring(0, 3).ToLower();
				if (dow.Length != 3)
				{
					throw new Exception("Unable to generate three letter DOW");
				}

				//the lengths of each section
				// ReSharper disable once RedundantExplicitArrayCreation
				var lengths = new int[]
				{
					5 + (date.Month % 3),
					3 + (date.Day % 3),
					8 + (date.Year % 3)
				};

				//generate the section checks
				var sectionChecks = new List<List<Func<string, bool>>>()
				{
					new List<Func<string, bool>>()
						{
							(s) => s.Length == lengths[0],		//required length
							(s) => s[s.Length - 1] == dow[2],	//last letter is last letter in DOW
							(s) => s.Contains(Code[0]),		//must contain the first char in Code
							(s) => Secret.ToLower().Any(s.Contains),	//must contain any char in Secret
						},
					new List<Func<string, bool>>()
						{
							(s) => s.Length == lengths[1],		//required length
							(s) => s[s.Length - 1] == dow[1],	//last letter is middle letter in DOW
							(s) => Secret.ToLower().Any(s.Contains),	//must contain any char in Secret

						},
					new List<Func<string, bool>>()
						{
							(s) => s.Length == lengths[2],		//required length
							(s) => s[0] == dow[0],				//first letter is first letter in DOW
							(s) => s.Contains(Code[1]),		//must contain the second char in Code
							(s) => s.Contains(Code[2]),		//must contain the third char in Code
							(s) => Secret.ToLower().Any(s.Contains),	//must contain any char in Secret
							//must contain the first three chars in User
							(s) => user.Substring(0,3).ToLower().All(s.Contains),		
						}
				};

				var result = true;

				for (int i = 0; i < parts.Length && result; i++)
				{
					var section = parts[i];
					var checks = sectionChecks[i];
					for (int j = 0; j < checks.Count && result; j++)
					{
						result = checks[j](section);
					}
				}

				return result;
			}

			#endregion

		}
	}
}
