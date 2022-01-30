using BanningApplications.WebApi.Data;
using BanningApplications.WebApi.Entities.settings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Repo.settings
{
	public interface IUserSettingsRepository: IDataRepository
	{
		//getters
		Task<UserSettings> GetAsync(string userId, string scope, bool asTracked = false);

		//setters
		Task<UserSettings> CreateAsync(UserSettings model);

		UserSettings UpdateEntity(UserSettings entity, bool isNew = false);
	}

    public class UserSettingsRepository: IUserSettingsRepository
    {
		private readonly SettingsDbContext _context;

		public UserSettingsRepository(
			SettingsDbContext context)
		{
			_context = context;
		}

		#region >> HELPERS <<

		protected virtual IQueryable<UserSettings> Query(bool asNoTracking = true)
		{
			var query = asNoTracking ? _context.Set<UserSettings>().AsNoTracking() : _context.Set<UserSettings>();

			return query;
		}


		#endregion


		#region >> GETTERS <<

		public async Task<UserSettings> GetAsync(string userId, string scope, bool asTracked = false)
		{
			return await Query(!asTracked)
							.SingleOrDefaultAsync(m => m.UserId == userId && m.Scope == scope);
		}

		#endregion

		#region >> SETTERS <<

		public async Task<UserSettings> CreateAsync(UserSettings model)
		{
			if (model == null) { throw new ArgumentNullException(nameof(model)); }

			await _context.Set<UserSettings>().AddAsync(UpdateEntity(model, true));

			return model;

		}

		public UserSettings UpdateEntity(UserSettings entity, bool isNew = false)
		{
			if (entity == null) { throw new ArgumentNullException(nameof(entity)); }

			entity.ModifyDate = DateTime.UtcNow;
			if (isNew) { entity.CreateDate = DateTime.UtcNow; }

			return entity;

		}

		public async Task<int> SaveAsync()
		{
			return await _context.SaveChangesAsync();
		}
		#endregion
	}
}
