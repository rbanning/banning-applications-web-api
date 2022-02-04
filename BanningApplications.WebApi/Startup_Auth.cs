using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BanningApplications.WebApi
{
    //Authorization and Authentication
    public static partial class StartupHelper
    {
	    public static void ConfigureAuth(IServiceCollection services, IConfiguration configuration)
	    {
            
		    services.AddAuthentication(options =>
			    {
				    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			    })
			    .AddJwtBearer(options =>
			    {
				    options.SaveToken = true;

				    options.TokenValidationParameters = new TokenValidationParameters
				    {
					    ValidateIssuer = true,
					    ValidateAudience = true,
					    ValidateLifetime = true,
					    ValidateIssuerSigningKey = true,
					    ValidIssuer = configuration[ConfigKeys.JwtIssuer],
					    ValidAudience = configuration[ConfigKeys.JwtIssuer],
					    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration[ConfigKeys.JwtKey]))
				    };
			    });

		    Identity.AuthHelper.ConfigureAuthorization(services, configuration);

	    }
    }
}
