// ReSharper disable InconsistentNaming
namespace BanningApplications.WebApi.Identity
{
    public class RegisteredClaimNames
    {
		private const string PRE = "my/";
		public const string Id = PRE + "id";
		public const string SecurityStamp = PRE + "chk";
		public const string Name = PRE + "name";
		public const string Email = PRE + "email";
		public const string Scope = PRE + "scope";
		public const string ServiceId = PRE + "serviceid";
		public const string Role = PRE + "role";

	}
}
