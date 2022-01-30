using BanningApplications.WebApi.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
// ReSharper disable InconsistentNaming

namespace BanningApplications.WebApi.Identity
{
	public static class JwtExtensions
	{
		public static int TTL (this IConfiguration _config)
		{
			if (_config != null && int.TryParse(_config[ConfigKeys.JwtTTL], out int ttl))
			{
				return ttl;
			}
			//else
			return -1;
		}

		public static DateTime? AuthExpires(this IConfiguration _config)
		{
			int ttl = _config.TTL();
			if (ttl > 0)
			{
				return DateTime.UtcNow.AddMinutes(ttl);
			}
			//else
			return null;
		}

		public static string ToAuthToken(this AppUser user, IConfiguration _config)
		{
			var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config[ConfigKeys.JwtKey]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var expires = _config.AuthExpires();
			if (expires.HasValue)
			{
				var token = new JwtSecurityToken(_config[ConfigKeys.JwtIssuer],
				  _config[ConfigKeys.JwtIssuer],
				  user.GenerateClaims(),
				  expires: expires.Value,
				  signingCredentials: creds);

				return new JwtSecurityTokenHandler().WriteToken(token);
			}

			//else
			return string.Empty;
		}

		public static Claim[] GenerateClaims(this AppUser user)
		{
			return user.ClaimsDictionary()
					.Select(m => new Claim(m.Key, m.Value))
					.ToArray();
		}

		public static ClaimsIdentity ToClaimsIdentity(this AppUser user)
		{
			var identity = new ClaimsIdentity();
			identity.AddClaims(user.GenerateClaims());
			return identity;
		}

		public static string FindClaimValue(this ClaimsIdentity identity, string type, string defaultValue = null)
		{
			if (identity == null) { return defaultValue; }
			var claim = identity.Claims.FirstOrDefault(m => m.Type == type);
			return claim == null ? defaultValue : claim.Value;
		}
		public static string FindClaimValue(this ClaimsPrincipal identity, string type, string defaultValue = null)
		{
			if (identity == null) { return defaultValue; }
			var claim = identity.Claims.FirstOrDefault(m => m.Type == type);
			return claim == null ? defaultValue : claim.Value;
		}

		public static bool IsUserInRole(ClaimsPrincipal user, string role)
		{
			if (user != null && user.HasClaim(c => c.Type == RegisteredClaimNames.Role))
			{
				return string.Equals(user.FindFirstValue(RegisteredClaimNames.Role), role, StringComparison.CurrentCultureIgnoreCase);
			}

			//else
			return false;
		}

		public static AppUser ToAppUser(this ClaimsPrincipal identity)
		{
			if (identity != null && identity.Claims != null)
			{
				return new AppUser()
				{
					Id = identity.FindClaimValue(RegisteredClaimNames.Id).Decrypt(),
					Name = identity.FindClaimValue(RegisteredClaimNames.Name),
					Email = identity.FindClaimValue(RegisteredClaimNames.Email),
					Role = identity.FindClaimValue(RegisteredClaimNames.Role),
					Scope = identity.FindClaimValue(RegisteredClaimNames.Scope),
					TrelloId = identity.FindClaimValue(RegisteredClaimNames.ServiceId),
					SecurityStamp = identity.FindClaimValue(RegisteredClaimNames.SecurityStamp).Decrypt()
				};
			}

			//else
			return null;
		}


		private static Dictionary<string, string> ClaimsDictionary(this AppUser user)
		{
			return new Dictionary<string, string>() {
				{ RegisteredClaimNames.Id, user.Id.Encrypt() },
				{ RegisteredClaimNames.Name, user.Name },
				{ RegisteredClaimNames.Email, user.Email },
				{ RegisteredClaimNames.SecurityStamp, user.SecurityStamp.Encrypt() },
				{ RegisteredClaimNames.Role, user.Role ?? "" },
				{ RegisteredClaimNames.ServiceId, user.TrelloId ?? "" }, 
				{ RegisteredClaimNames.Scope, user.Scope ?? "" }
			};
		}


	}
}
