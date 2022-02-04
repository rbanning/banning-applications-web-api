using System.Reflection;
//using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BanningApplications.WebApi
{
	public class Startup
	{
		private readonly string _corsPolicyName;

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			_corsPolicyName = Configuration[ConfigKeys.CorsPolicyName];
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

			// *** AutoMapper ***
			StartupHelper.ConfigureAutoMapper(services);
			
			// *** Setup HttpClientFactory ***
			StartupHelper.ConfigureHttpClientFactory(services);

			// *** Setup CORS *** 
			StartupHelper.ConfigureCors(services, Configuration, _corsPolicyName);

			// *** Setup MVC (controllers) ***
			StartupHelper.ConfigureControllers(services);

			// *** Configure Identity ***
			StartupHelper.ConfigureIdentity(services, Configuration, migrationAssembly);

			// *** Configure Authentication & Authorization ***
			StartupHelper.ConfigureAuth(services, Configuration);


			// *** Services *** //
			StartupHelper.ConfigureServices(services, Configuration);


			// *** Configure Data Contexts and Repositories ***
			StartupHelper.ConfigureDbContextAndRepositories(services, Configuration, migrationAssembly);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseExceptionHandler("/error/dev");
			}
			else
			{
				app.UseExceptionHandler("/error");

				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			if (!string.IsNullOrEmpty(_corsPolicyName)) { app.UseCors(_corsPolicyName); }
			
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();	//must be before UseAuthentication and UseAuthorization
			app.UseAuthentication();
			app.UseAuthorization();

			//migration
			//app.UseMvc();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			app.Run(async (context) =>
			{
				var isRoot = !context.Request.Path.HasValue || string.Equals(context.Request.Path.Value, "/");
				if (isRoot)
				{
					await context.Response.WriteAsync("Banning Applications");
				} else
				{
					context.Response.StatusCode = 404;
				}
			});
		}
	}
}
