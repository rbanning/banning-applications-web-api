using BanningApplications.WebApi.Entities.unsplash;
using Microsoft.EntityFrameworkCore;

namespace BanningApplications.WebApi.Data
{
    public class UnsplashDbContext: DbContext
    {
	    public UnsplashDbContext(DbContextOptions<UnsplashDbContext> options)
			:base(options)
	    { }

	    public DbSet<UnsplashPhotographer> UnsplashPhotographers { get; set; }

	    public DbSet<UnsplashPhoto> UnsplashPhotos { get; set; }

	    public DbSet<UnsplashAwardWinner> UnsplashAwardWinners { get; set; }

	    protected override void OnModelCreating(ModelBuilder modelBuilder)
	    {
		    modelBuilder.Entity<UnsplashPhotographer>()
			    .HasAlternateKey(m => m.UserName);

		    modelBuilder.Entity<UnsplashPhoto>()
			    .HasOne(m => m.Photographer)
			    .WithMany()
			    .HasForeignKey(m => m.UserName)
			    .HasPrincipalKey(m => m.UserName);

		    modelBuilder.Entity<UnsplashAwardWinner>()
			    .HasOne(m => m.Photo)
			    .WithMany()
			    .HasForeignKey(m => m.PhotoId);

	    }
    }
}
