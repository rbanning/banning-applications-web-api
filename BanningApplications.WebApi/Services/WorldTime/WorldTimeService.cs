using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BanningApplications.WebApi.Helpers;
using BanningApplications.WebApi.Services.Trello;

namespace BanningApplications.WebApi.Services.WorldTime
{
	public interface IWorldTimeService
	{
		Task<WorldTime> CurrentByCoordinates(float latitude, float longitude);
	}
    public class WorldTimeService: IWorldTimeService
    {
	    private const string HTTP_NAME = "world_time";

		private readonly IHttpClientFactory _clientFactory;

		public WorldTimeService(IHttpClientFactory clientFactory)
		{
			_clientFactory = clientFactory;
		}

		#region >> Current Time <<


		public async Task<WorldTime> CurrentByCoordinates(float latitude, float longitude)
		{
			var url = BuildUrl("Time/current/coordinate", ("latitude", $"{latitude}"), ("longitude", $"{longitude}"));
			return await PerformGetAsync<WorldTime>(url);
		}


		#endregion

		#region >> HELPERS <<

		protected async Task<T> PerformGetAsync<T>(string url)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			var client = _clientFactory.CreateClient(HTTP_NAME);
			var response = await client.SendAsync(request);
			
			//DEBUG:
			Console.WriteLine("World Service -- performing get {0}", url);

			return await ProcessResponse<T>(response);
		}

		protected async Task<T> ProcessResponse<T>(HttpResponseMessage response)
		{
			if (response.IsSuccessStatusCode)
			{
				try
				{
					var json = await response.Content.ReadAsStringAsync();
					if (!string.IsNullOrEmpty(json))
					{

						return JsonSerializer.Deserialize<T>(json, SerializerOptions());
					}
				}
				catch (Exception ex)
				{
					throw new TrelloServiceException("Unable to deserialize response", ex);
				}
			}
			else if (response.StatusCode != HttpStatusCode.NotFound)
			{
				//todo: add more information about the response?
				//var requestMessage = response.RequestMessage.Serialize();
				//var requestHeaders = response.RequestMessage.Headers.Serialize();
				//var requestContent = response.RequestMessage.Content.Serialize();

				await ThrowServiceError(response);
			}

			//else
			return default(T);
		}


		protected string BuildUrl(string path, params (string key, string value)[] queryStrings)
		{
			var url = $"{path}";
			if (queryStrings.Length > 0)
			{
				url += $"?{string.Join("&", queryStrings.Select(pair => $"{pair.key}={pair.value}"))}";
			}
			return url;
		}

		protected JsonSerializerOptions SerializerOptions()
		{
			return SerializerHelpers.Options();
		}

		protected async Task ThrowServiceError(HttpResponseMessage response)
		{
			Exception terror;
			try
			{
				terror = new Exception(await response.Content.ReadAsStringAsync());
			}
			catch (Exception ex)
			{
				terror = ex;
			}

			throw new WorldTimeException($"Problem with World Time Request - {response.StatusCode}", terror);
		}

		#endregion

    }
}
