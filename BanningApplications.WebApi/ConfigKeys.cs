using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi
{
    public class ConfigKeys
    {
		private const string Jwt = "Jwt";
		public const string JwtIssuer = Jwt + ":Issuer";
		public const string JwtKey = Jwt + ":Key";
		public const string JwtTTL = Jwt + ":TTL";

		public const string CorsOrigins = "CORS:origins";
		public const string CorsPolicyName = "CORS:policy";

		public const string Email = "Email";
		public const string EmailApi = Email + ":Api";

		public const string DbSettings = "settings";
		public const string DbTrelloData = "trellodata";
		public const string DbDocket = "docket";
		public const string DbAussieadmin = "aussieadmin";
		public const string DbHook = "hook";
		public const string DbNotifications = "notifications";
		public const string DbTRG = "trg";
		public const string DbTTFF = "ttff";
	}
}
