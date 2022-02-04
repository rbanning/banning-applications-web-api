using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
//using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using BanningApplications.WebApi.Repo.unsplash;
using BanningApplications.WebApi.Services.Blob;
using BanningApplications.WebApi.Services.Queue;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
			// *** AutoMapper ***
			var mappingConfig = new AutoMapper.MapperConfiguration(mc =>
			{
				mc.AddProfile(new Dtos.Identity.IdentityMappingProfile());
				mc.AddProfile(new Dtos.settings.settingsMappingProfile());
				mc.AddProfile(new Dtos.hook.hookMappingProfile());
				mc.AddProfile(new Dtos.unsplash.unsplashMappingProfile());
			});
			services.AddSingleton(mappingConfig.CreateMapper());

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

			// *** Setup CORS *** 
			services.AddCors(options =>
			{
				var origins = Configuration[ConfigKeys.CorsOrigins];

				if (!string.IsNullOrEmpty(origins) && !string.IsNullOrEmpty(_corsPolicyName))
				{

					options.AddPolicy(_corsPolicyName, builder =>
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

			// *** Setup MVC ***
			services.AddControllers()
				.SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
					options.JsonSerializerOptions.IgnoreNullValues = true;
					options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
				});


			// *** Configure Identity ***
			var identityConnectionString = Configuration.GetConnectionString("Identity");
			var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
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



			// *** Configure Authentication & Authorization ***

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
						ValidIssuer = Configuration[ConfigKeys.JwtIssuer],
						ValidAudience = Configuration[ConfigKeys.JwtIssuer],
						IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Configuration[ConfigKeys.JwtKey]))
					};
				});

			Identity.AuthHelper.ConfigureAuthorization(services, Configuration);


			// *** Configure Azure Storage Services *** //
			services.AddSingleton(x => new BlobServiceClient(Configuration.GetConnectionString("AzureBlobStorage")));
			services.AddSingleton(x => new QueueServiceClient(Configuration.GetConnectionString("AzureBlobStorage")));

			services.AddSingleton<IBlobService, BlobService>();
			services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(opt =>
			{
				opt.MultipartBodyLengthLimit = BlobService.MAX_CONTENT_LENGTH;
			});
			services.AddSingleton<IQueueService, QueueService>();


			// *** Configure Other Services *** //
			services.AddScoped<Services.Google.IGooglePlaces, Services.Google.GooglePlaces>();
			services.AddScoped<Services.File.IFileService, Services.File.FileService>();
			services.AddScoped<Services.Trello.ITrelloService, Services.Trello.TrelloService>();
			services.AddScoped<Services.Slack.ISlackService, Services.Slack.SlackService>();
			services.AddScoped<Services.PDF.IPdfService, Services.PDF.PdfService>();
			services.AddScoped<Services.WorldTime.IWorldTimeService, Services.WorldTime.WorldTimeService>();


			// *** Configure Data Contexts and Repositories ***



			// -- settings
			services.AddDbContext<Data.SettingsDbContext>(options =>
			{
				options.UseSqlServer(Configuration.GetConnectionString(ConfigKeys.DbSettings),
					sql => sql.MigrationsAssembly(migrationAssembly));
			});
			services.AddScoped<Repo.settings.IUserSettingsRepository, Repo.settings.UserSettingsRepository>();

			// -- hook
			services.AddDbContext<Data.HookDbContext>(options =>
			{
				options.UseSqlServer(Configuration.GetConnectionString(ConfigKeys.DbHook),
					sql => sql.MigrationsAssembly(migrationAssembly));
			});
			services.AddScoped<Repo.hook.IHookRequestRepository, Repo.hook.HookRequestRepository>();

			// -- unsplash
			services.AddDbContext<Data.UnsplashDbContext>(options =>
			{
				options.UseSqlServer(Configuration.GetConnectionString(ConfigKeys.DbUnsplash),
					sql => sql.MigrationsAssembly(migrationAssembly));
			});
			services.AddScoped<IUnsplashPhotographersRepository, UnsplashPhotographersRepository>();

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
