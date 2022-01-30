using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Identity
{
	public enum KnownUserToken
	{
		confirm_email,
		confirm_phone,
		reset_password
	}

	public interface IAppUserTokenRepository
	{
		Task<AppUserToken> FindAsync(string userId, string scope, string type);
		Task<AppUserToken> FindAsync(string userId, string scope, KnownUserToken type);
		Task<AppUserToken> CreateOrUpdateAsync(string userId, string scope, string type, string token, int ttlMinutes);
		Task<AppUserToken> CreateOrUpdateAsync(string userId, string scope, KnownUserToken type, string token, int ttlMinutes);
		Task<bool> DeleteAsync(string userId, string scope, string type);
		Task<bool> DeleteAsync(string userId, string scope, KnownUserToken type);

		string GenerateTokenString(int length);
		int KnownUserTokenLength(KnownUserToken type);
		int KnownUserTokenTTL(KnownUserToken type);

		Task<AppUserToken> GenerateTokenAsync(string userId, string scope, KnownUserToken type);
		Task<AppUserToken> GenerateTokenAsync(string userId, string scope, string type, int length, int ttlMinutes);

		Task<bool> ValidateAndDeleteAsync(string userId, string scope, string type, string token);
		Task<bool> ValidateAndDeleteAsync(string userId, string scope, KnownUserToken type, string token);

	}
	public class AppUserTokenRepository: IAppUserTokenRepository
    {
		private AppIdentityDbContext _context;

		public AppUserTokenRepository(AppIdentityDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<AppUserToken> FindAsync(string userId, string scope, string type)
		{
			return await _context.AppUserTokens.FindAsync(userId, scope, type);
		}
		public async Task<AppUserToken> FindAsync(string userId, string scope, KnownUserToken type)
		{
			return await FindAsync(userId, scope, type.ToString());
		}

		public async Task<AppUserToken> CreateOrUpdateAsync(string userId, string scope, string type, string token, int ttlMinutes)
		{
			var entity = await FindAsync(userId, scope, type);
			if (entity == null)
			{
				entity = new AppUserToken()
				{
					UserId = userId,
					Scope = scope,
					Type = type,
					Token = token
				};
				entity.SetExpiration(ttlMinutes);
				await _context.AppUserTokens.AddAsync(entity);
			} else
			{
				entity.Token = token;
				entity.SetExpiration(ttlMinutes);
			}
			await _context.SaveChangesAsync();
			return entity;
		}

		public async Task<AppUserToken> CreateOrUpdateAsync(string userId, string scope, KnownUserToken type, string token, int ttlMinutes)
		{
			return await CreateOrUpdateAsync(userId, scope, type.ToString(), token, ttlMinutes);
		}

		public async Task<bool> DeleteAsync(string userId, string scope, string type)
		{
			var entity = await FindAsync(userId, scope, type);
			if (entity == null)
			{
				return false;
			}
			//else
			_context.AppUserTokens.Remove(entity);
			await _context.SaveChangesAsync();
			return true;
		}
		public async Task<bool> DeleteAsync(string userId, string scope, KnownUserToken type)
		{
			return await DeleteAsync(userId, scope, type.ToString());
		}

		public string GenerateTokenString(int length)
		{
			return RandomString(length);
		}
		public int KnownUserTokenLength(KnownUserToken type)
		{
			switch (type)
			{
				case KnownUserToken.confirm_email:
				case KnownUserToken.confirm_phone:
					return 6;
				case KnownUserToken.reset_password:
					return 7;
				default:
					return 5;
			}
		}

		public int KnownUserTokenTTL(KnownUserToken type)
		{
			switch (type)
			{
				case KnownUserToken.confirm_email:
					return 60;
				case KnownUserToken.confirm_phone:
					return 60;
				case KnownUserToken.reset_password:
					return 60;
				default:
					return 15;
			}
		}


		public async Task<AppUserToken> GenerateTokenAsync(string userId, string scope, string type, int length, int ttlMinutes)
		{
			var token = GenerateTokenString(length);
			return await CreateOrUpdateAsync(userId, scope, type, token, ttlMinutes);
		}

		public async Task<AppUserToken> GenerateTokenAsync(string userId, string scope, KnownUserToken type)
		{
			int length = KnownUserTokenLength(type);
			int ttlMinutes = KnownUserTokenTTL(type);
			return await GenerateTokenAsync(userId, scope, type.ToString(), length, ttlMinutes);
		}


		public async Task<bool> ValidateAndDeleteAsync(string userId, string scope, string type, string token)
		{
			var entity = await FindAsync(userId, scope, type);
			if (entity == null || !string.Equals(entity.Token, token, StringComparison.CurrentCultureIgnoreCase))
			{
				return false;
			}
			//else
			_context.AppUserTokens.Remove(entity);
			await _context.SaveChangesAsync();

			//TODO - check to see if the token has expired
			return true;

		}

		public async Task<bool> ValidateAndDeleteAsync(string userId, string scope, KnownUserToken type, string token)
		{
			return await ValidateAndDeleteAsync(userId, scope, type.ToString(), token);
		}


		private string RandomString(int length)
		{
			StringBuilder str = new StringBuilder();
			Random random = new Random();

			int A = Encoding.ASCII.GetBytes("A").First();	//could hardcode it as 65

			char letter;

			for (int i = 0; i < length; i++)
			{
				double flt = random.NextDouble();
				int shift = Convert.ToInt32(Math.Floor(25 * flt));
				letter = Convert.ToChar(shift + A);
				str.Append(letter);
			}

			return str.ToString();
		}
	}
}
