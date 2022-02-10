using BanningApplications.WebApi.Repo.unsplash;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BanningApplications.WebApi
{
    //db context and repositories
    public static partial class StartupHelper
    {
	    public static void ConfigureDbContextAndRepositories(IServiceCollection services, IConfiguration configuration, string migrationAssembly)
	    {
		    // -- settings
		    services.AddDbContext<Data.SettingsDbContext>(options =>
		    {
			    options.UseSqlServer(configuration.GetConnectionString(ConfigKeys.DbSettings),
				    sql => sql.MigrationsAssembly(migrationAssembly));
		    });
		    services.AddScoped<Repo.settings.IUserSettingsRepository, Repo.settings.UserSettingsRepository>();

		    // -- hook
		    services.AddDbContext<Data.HookDbContext>(options =>
		    {
			    options.UseSqlServer(configuration.GetConnectionString(ConfigKeys.DbHook),
				    sql => sql.MigrationsAssembly(migrationAssembly));
		    });
		    services.AddScoped<Repo.hook.IHookRequestRepository, Repo.hook.HookRequestRepository>();

		    // -- unsplash
		    services.AddDbContext<Data.UnsplashDbContext>(options =>
		    {
			    options.UseSqlServer(configuration.GetConnectionString(ConfigKeys.DbUnsplash),
				    sql => sql.MigrationsAssembly(migrationAssembly));
		    });
		    services.AddScoped<IUnsplashPhotographersRepository, UnsplashPhotographersRepository>();
		    services.AddScoped<IUnsplashPhotosRepository, UnsplashPhotosRepository>();
		    services.AddScoped<IUnsplashAwardWinnersRepository, UnsplashAwardWinnersRepository>();
		    services.AddScoped<IGameScoresRepository, GameScoresRepository>();
	    }
    }
}
