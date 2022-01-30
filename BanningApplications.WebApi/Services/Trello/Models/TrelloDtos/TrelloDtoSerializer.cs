using System.Text.Json;

namespace BanningApplications.WebApi.Services.Trello.Models.TrelloDtos
{
    public static class TrelloDtoSerializer
    {
	    public static string Serialize<T>(T model)
	    {
		    return JsonSerializer.Serialize(model, new JsonSerializerOptions()
		    {
			    WriteIndented = true,
			    IgnoreNullValues = true,
			    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		    });
		}

	    public static T Deserialize<T>(string json) where T : class
	    {
		    return JsonSerializer.Deserialize<T>(json, Helpers.SerializerHelpers.OptionsWithNamingPolicy());
	    }
    }
}
