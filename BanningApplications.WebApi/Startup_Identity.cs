using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BanningApplications.WebApi
{
    //identity
    public static partial class StartupHelper
    {
	    public static void ConfigureIdentity(IServiceCollection services, IConfiguration configuration, string migrationAssembly)
	    {
		    // *** Configure Identity ***
		    var identityConnectionString = configuration.GetConnectionString("Identity");
		    services.AddDbContext<Identity.AppIdentityDbContext>(options => options.UseSqlServer(identityConnectionString,
			    sql => sql.MigrationsAssembly(migrationAssembly)));

		    services.AddIdentity<Identity.AppUser, IdentityRole>(options =>
			    {
				    //ISSUE #4 - updated identity options 

				    //email validation
				    options.User.RequireUniqueEmail = true;
				    options.SignIn.RequireConfirmedEmail = false;

				    //lockout
				    options.Lockout.AllowedForNewUsers = true;
				    options.Lockout.MaxFailedAccessAttempts = 6;
				    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);

				    //password
				    options.Password.RequiredLength = 8;
				    options.Password.RequireDigit = false;
				    options.Password.RequireNonAlphanumeric = false;	//no need for a non-alphanumeric
				    options.Password.RequireLowercase = true;
				    options.Password.RequireUppercase = true;
			    })
			    .AddEntityFrameworkStores<Identity.AppIdentityDbContext>()
			    .AddDefaultTokenProviders() //brings in password reset token provider
			    .AddTokenProvider<Identity.EmailConfirmationTokenProvider<Identity.AppUser>>("emailconf");

		    services.AddScoped<IUserStore<Identity.AppUser>, UserOnlyStore<Identity.AppUser, Identity.AppIdentityDbContext>>();
		    services.AddScoped<Identity.IAppUserScopedRolesRepository, Identity.AppUserScopedRolesRepository>();
		    services.AddScoped<Identity.IAppUserTokenRepository, Identity.AppUserTokenRepository>();

		    services.Configure<DataProtectionTokenProviderOptions>(options =>
		    {
			    options.TokenLifespan = TimeSpan.FromHours(3);
		    });
		    services.Configure<Identity.EmailConfirmationTokenProviderOptions>(options =>
		    {
			    options.TokenLifespan = TimeSpan.FromDays(3);
		    });


	    }
    }
}
