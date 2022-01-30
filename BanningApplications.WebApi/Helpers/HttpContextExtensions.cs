using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Helpers
{
    public static class HttpContextExtensions
    {
	    public static async Task<T> ReadAsAsync<T>(this HttpContent content)
	    {
		    return await JsonSerializer.DeserializeAsync<T>(
				await content.ReadAsStreamAsync(),
				SerializerHelpers.Options()
			);
	    }
    }
}
