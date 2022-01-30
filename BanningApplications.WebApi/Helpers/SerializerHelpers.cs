using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BanningApplications.WebApi.Helpers
{
    public static class SerializerHelpers
    {
	    public static JsonSerializerOptions Options()
	    {
			return new JsonSerializerOptions()
			{
				PropertyNameCaseInsensitive = true,
				Converters =
				{
					new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
				}
			};
	    }

	    public static JsonSerializerOptions OptionsWithNamingPolicy()
	    {
			return new JsonSerializerOptions()
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				PropertyNameCaseInsensitive = true,
				Converters =
				{
					new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
				}
			};
	    }


	    public static string Serialize<T>(this T instance)
	    {
		    try
		    {
			    return JsonSerializer.Serialize(instance, OptionsWithNamingPolicy());
		    }
		    catch (Exception ex)
		    {
			    throw new Exception("Error trying to serialize object", ex);
		    }
	    }
	    public static T Deserialize<T>(this string json)
	    {
		    try
		    {
			    return JsonSerializer.Deserialize<T>(json, OptionsWithNamingPolicy());
		    }
		    catch (Exception ex)
		    {
			    throw new Exception("Error trying to deserialize object", ex);
		    }
	    }

	}
}
