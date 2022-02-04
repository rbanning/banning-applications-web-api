using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BanningApplications.WebApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BanningApplications.WebApi
{
	//CORS & Controllers (MVC)
	public static partial class StartupHelper 
    {
	    public static void ConfigureCors(IServiceCollection services, IConfiguration configuration, string corsPolicyName)
	    {
		    // *** Setup CORS *** 
		    services.AddCors(options =>
		    {
			    var origins = configuration[ConfigKeys.CorsOrigins];

			    if (!string.IsNullOrEmpty(origins) && !string.IsNullOrEmpty(corsPolicyName))
			    {

				    options.AddPolicy(corsPolicyName, builder =>
				    {
					    //** Migration to 3.1 using AllowAnyOrigin
					    //InvalidOperationException: The CORS protocol does not allow specifying a wildcard (any) origin and credentials at the same time. Configure the CORS policy by listing individual origins if credentials needs to be supported.
					    //builder
					    //	.AllowAnyMethod()
					    //	.AllowAnyHeader()
					    //	.AllowAnyOrigin()
					    //	.AllowCredentials();

					    builder
						    .AllowAnyMethod()
						    .AllowAnyHeader()
						    .SetIsOriginAllowed(origin => true)
						    .AllowCredentials();

					    //** Had Trouble accessing PDF so am trying the code above [2021-01-15]
					    //builder.WithOrigins(origins.Split(","))
					    //.AllowAnyMethod()
					    //.AllowAnyHeader()
					    //.AllowCredentials(); 
					    ////might need to remove .AllowCredentials()
					    ////see
					    ////https://docs.microsoft.com/en-us/aspnet/core/migration/21-to-22?view=aspnetcore-2.2&tabs=visual-studio#update-cors-policy

				    });

			    }
		    });

	    }

	    public static void ConfigureControllers(IServiceCollection services)
	    {
		    // *** Setup MVC ***
		    services.AddControllers()
			    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
			    .AddJsonOptions(options =>
			    {
				    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
				    options.JsonSerializerOptions.IgnoreNullValues = true;
				    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
			    });
		    services.AddControllers(o => o.InputFormatters.Insert(o.InputFormatters.Count, new TextPlainInputFormatter()));

	    }
    }
}
