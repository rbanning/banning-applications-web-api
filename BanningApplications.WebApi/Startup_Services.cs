using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using BanningApplications.WebApi.Services.Blob;
using BanningApplications.WebApi.Services.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BanningApplications.WebApi
{
    //services
    public static partial class StartupHelper
    {
	    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	    {
		    // *** Configure Azure Storage Services *** //
		    services.AddSingleton(x => new BlobServiceClient(configuration.GetConnectionString("AzureBlobStorage")));
		    services.AddSingleton(x => new QueueServiceClient(configuration.GetConnectionString("AzureBlobStorage")));

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


	    }
    }
}
