using BanningApplications.WebApi.Entities.settings;
using Microsoft.EntityFrameworkCore;

namespace BanningApplications.WebApi.Data
{
    public class SettingsDbContext: DbContext
    {
		public SettingsDbContext(DbContextOptions<SettingsDbContext> options)
			:base(options)
		{ }

		public DbSet<UserSettings> UserSettings { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<UserSettings>()
				.HasKey(m => new { m.UserId, m.Scope });
		}


	}
}
