using System;
using Microsoft.Extensions.DependencyInjection;

namespace BanningApplications.WebApi
{
	//AutoMapper and HttpClientFactory
    public static partial class StartupHelper 
    {
	    public static void ConfigureAutoMapper(IServiceCollection services)
	    {
		    var mappingConfig = new AutoMapper.MapperConfiguration(mc =>
		    {
			    mc.AddProfile(new Dtos.Identity.IdentityMappingProfile());
			    mc.AddProfile(new Dtos.settings.settingsMappingProfile());
			    mc.AddProfile(new Dtos.hook.hookMappingProfile());
			    mc.AddProfile(new Dtos.unsplash.unsplashMappingProfile());
		    });
		    services.AddSingleton(mappingConfig.CreateMapper());
	    }

	    public static void ConfigureHttpClientFactory(IServiceCollection services)
	    {
		    // *** Setup HttpClientFactory ***
		    services.AddHttpClient("google", c =>
		    {
			    c.BaseAddress = new Uri("https://www.googleapis.com/");
		    });
		    services.AddHttpClient("maps_google", c =>
		    {
			    c.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/");
		    });
		    services.AddHttpClient("trello", c =>
		    {
			    c.BaseAddress = new Uri("https://api.trello.com/1/");
		    });
		    services.AddHttpClient("slack", c =>
		    {
			    c.BaseAddress = new Uri("https://hooks.slack.com/");
		    });
		    services.AddHttpClient("blob", c =>
		    {
			    c.BaseAddress = new Uri("https://hallpasscloudstoragev2.blob.core.windows.net/");
		    });
		    services.AddHttpClient("localhost", c =>
		    {
			    c.BaseAddress = new Uri("https://localhost:44309/");
		    });
		    services.AddHttpClient("world_time", c =>
		    {
			    c.BaseAddress = new Uri("https://www.timeapi.io/api/");
		    });

	    }
    }
}
