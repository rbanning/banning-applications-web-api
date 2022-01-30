using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Identity
{
    public static class RegisteredRoles
    {
		public const string viewer = "viewer";
		public const string customer = "customer";
		public const string manager = "manager";
		public const string admin = "admin";
		public const string root = "root";

		public static List<string> Names =>
			new List<string>()
			{
				viewer, customer, manager, admin, root
			};

		public static Dictionary<string, byte> PermissionLevels
		{
			get
			{
				var names = Names;
				var ret = new Dictionary<string, byte>();
				for (byte i = 0; i < names.Count; i++)
				{
					ret.Add(names[i], i);
				}
				return ret;
			}
		}

		public static byte PermissionLevel(string role)
		{
			var dict = PermissionLevels;
			if (role == null || !dict.ContainsKey(role)) { throw new ArgumentException("Invalid permission level role: " + (role ?? "null")); }
			return PermissionLevels.FirstOrDefault(kvp => kvp.Key == role).Value;
		}
	}
}
