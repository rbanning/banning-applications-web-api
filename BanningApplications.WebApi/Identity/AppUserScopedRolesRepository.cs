using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Identity
{
	public interface IAppUserScopedRolesRepository
	{
		Task<IEnumerable<AppUserScopedRole>> GetAllAsync(string scope);
		Task<IEnumerable<AppUserScopedRole>> GetAllBelongingToAsync(string userId);
		Task<AppUserScopedRole> FindAsync(string userId, string scope);
		Task<AppUserScopedRole> CreateOrUpdateAsync(string userId, string scope, string role, string avatar);
		Task<AppUserScopedRole> UpdateAvatarAsync(string userId, string scope, string avatar);
		Task<AppUserScopedRole> UpdateRoleAsync(string userId, string scope, string role);
		Task<AppUserScopedRole> UpdateTrelloIdAsync(string userId, string scope, string trelloId);
		Task<bool> DeleteAsync(string userId, string scope);
	}

	public class AppUserScopedRolesRepository : IAppUserScopedRolesRepository
	{
		private AppIdentityDbContext _context;

		public AppUserScopedRolesRepository(AppIdentityDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public async Task<IEnumerable<AppUserScopedRole>> GetAllAsync(string scope)
		{
			return await _context.AppUserScopedRoles.Where(m => m.Scope == scope).ToListAsync();
		}

		public async Task<IEnumerable<AppUserScopedRole>> GetAllBelongingToAsync(string userId)
		{
			return await _context.AppUserScopedRoles.Where(m => m.UserId == userId).ToListAsync();
		}

		public async Task<AppUserScopedRole> FindAsync(string userId, string scope)
		{
			return await _context.AppUserScopedRoles.FindAsync(userId, scope);
		}

		public async Task<AppUserScopedRole> CreateOrUpdateAsync(string userId, string scope, string role, string avatar = null)
		{
			var entity = await FindAsync(userId, scope);
			if (entity == null)
			{
				entity = new AppUserScopedRole()
				{
					UserId = userId,
					Scope = scope,
					Role = role,
					Avatar = avatar
				};
				await _context.AppUserScopedRoles.AddAsync(entity);
			} else
			{
				entity.Role = role;
				if (!string.IsNullOrEmpty(avatar)) { entity.Avatar = avatar; }
			}
			await _context.SaveChangesAsync();
			return entity;
		}

		public async Task<AppUserScopedRole> UpdateAvatarAsync(string userId, string scope, string avatar)
		{
			var entity = await FindAsync(userId, scope);
			if (entity == null)
			{
				throw new ArgumentException("Unknown app user - id and scope do not exist");
			} else
			{
				entity.Avatar = avatar;
			}
			await _context.SaveChangesAsync();
			return entity;
		}

		public async Task<AppUserScopedRole> UpdateRoleAsync(string userId, string scope, string role)
		{
			var entity = await FindAsync(userId, scope);
			if (entity == null)
			{
				throw new ArgumentException("Unknown app user - id and scope do not exist");
			} else
			{
				entity.Role = role;
			}
			await _context.SaveChangesAsync();
			return entity;
		}

		public async Task<AppUserScopedRole> UpdateTrelloIdAsync(string userId, string scope, string trelloId)
		{
			var entity = await FindAsync(userId, scope);
			if (entity == null)
			{
				throw new ArgumentException("Unknown app user - id and scope do not exist");
			} else
			{
				entity.TrelloId = trelloId;
			}
			await _context.SaveChangesAsync();
			return entity;
		}

		public async Task<bool> DeleteAsync(string userId, string scope)
		{
			var entity = await FindAsync(userId, scope);
			if (entity == null)
			{
				return false;
			}
			//else
			_context.AppUserScopedRoles.Remove(entity);
			await _context.SaveChangesAsync();
			return true;
		}

	}
}
