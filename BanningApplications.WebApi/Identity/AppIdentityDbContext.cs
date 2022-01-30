using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BanningApplications.WebApi.Identity
{
	public class AppIdentityDbContext : IdentityDbContext<AppUser>
	{

		public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
			: base(options)
		{ }


		public DbSet<AppUserScopedRole> AppUserScopedRoles { get; set; }
		public DbSet<AppUserToken> AppUserTokens { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<AppUserScopedRole>()
				.HasKey(r => new { r.UserId, r.Scope });

			builder.Entity<AppUserToken>()
				.HasKey(r => new { r.UserId, r.Scope, r.Type });
		}
	}

}
