using BanningApplications.WebApi.Entities.hook;
using Microsoft.EntityFrameworkCore;

namespace BanningApplications.WebApi.Data
{
    public class HookDbContext : DbContext
	{
		public HookDbContext(DbContextOptions<HookDbContext> options)
			: base(options)
		{ }

		public DbSet<HookRequest> HookRequests { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
		}
	}
}
