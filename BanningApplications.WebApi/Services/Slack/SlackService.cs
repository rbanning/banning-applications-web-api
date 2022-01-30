using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BanningApplications.WebApi.Helpers;
using BanningApplications.WebApi.Identity;
using BanningApplications.WebApi.Services.Trello;

namespace BanningApplications.WebApi.Services.Slack
{
    public class SlackService: ISlackService
    {
	    private const string HTTP_NAME = "slack";

	    private readonly IHttpClientFactory _clientFactory;
		private SlackConfig _config;

	    public SlackService(
		    IHttpClientFactory clientFactory)
	    {
		    _clientFactory = clientFactory;
	    }


		#region Configuration

		public bool IsConfigured => _config != null && _config.IsValid;

		public SlackConfig Config => _config;


		public void Configure(SlackConfig config)
	    {
		    _config = config;
	    }
	    public void Configure(RegisteredScopes.Scope scope)
	    {
		    _config = scope?.SlackConfig;
	    }
	    public void Configure(string scope /* can be scope id or scope code */)
	    {
		    Configure(RegisteredScopes.Find(scope) ?? RegisteredScopes.FindByCode(scope));
	    }


		#endregion


		#region Send Messages

		public async Task<string> SendMessageAsync(string channel, string message)
		{
			var url = BuildUrl(channel);
			var jsonContent = Serialize(new {text = message});
			return await PerformPostAsync<string>(url, jsonContent);
		}

		#endregion


		#region HELPERS 

		protected string BuildUrl(string channel, Dictionary<string, string> queryParams = null)
		{
			if (!IsConfigured)
			{
				throw new SlackServiceException("Slack service has not been configured");
			}

			var hook = Config.WebHooks.FirstOrDefault(m => string.Equals(m.Channel, channel, StringComparison.CurrentCultureIgnoreCase));

			if (hook == null)
			{
				throw new SlackServiceException("Slack service is not configured for the requested channel");
			}

			var url = $"services/{hook.Url}";
			if (queryParams != null)
			{
				foreach (var pair in queryParams)
				{
					url += $"&{pair.Key}={pair.Value}";
				}
			}
			return url;
		}

		protected async Task<T> PerformGetAsync<T>(string url)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			var client = _clientFactory.CreateClient(HTTP_NAME);
			var response = await client.SendAsync(request);

			return await ProcessResponse<T>(response);
		}

		protected async Task<T> PerformPostAsync<T>(string url, string serializedContent = null)
		{
			//serialize the 
			var content = serializedContent == null ? null : new StringContent(serializedContent, Encoding.UTF8, "application/json");
			var client = _clientFactory.CreateClient(HTTP_NAME);
			var response = await client.PostAsync(url, content);

			return await ProcessResponse<T>(response);
		}

		protected async Task<T> ProcessResponse<T>(HttpResponseMessage response)
		{
			if (response.IsSuccessStatusCode)
			{
				try
				{
					var json = await response.Content.ReadAsStringAsync();

					//if you are asking for a string
					if (typeof(T) == typeof(string))
					{
						return (T)Convert.ChangeType(json, typeof(T));
					}

					//else
					if (!string.IsNullOrEmpty(json))
					{
						var contentType = response.Content.Headers.FirstOrDefault(m => String.Equals(m.Key, "content-type", StringComparison.CurrentCultureIgnoreCase));
						if (contentType.Value.Any((m => m.Contains("application/json"))))
						{
							return JsonSerializer.Deserialize<T>(json, SerializerOptions());
						}

						//else, try to case the string response as the type
						return (T)Convert.ChangeType(json, typeof(T));
					}
				}
				catch (Exception ex)
				{
					throw new SlackServiceException("Unable to deserialize response", ex);
				}
			}
			else if (response.StatusCode != HttpStatusCode.NotFound)
			{
				//todo: add more information about the response?
				//var requestMessage = response.RequestMessage.Serialize();
				//var requestHeaders = response.RequestMessage.Headers.Serialize();
				//var requestContent = response.RequestMessage.Content.Serialize();

				await ThrowSlackServiceError(response);
			}

			//else
			return default(T);
		}

		protected async Task ThrowSlackServiceError(HttpResponseMessage response)
		{
			SlackError terror = null;
			try
			{
				var body = await response.Content.ReadAsStringAsync();
				if (!string.IsNullOrEmpty(body))
				{
					var contentType = response.Content.Headers.FirstOrDefault(m => String.Equals(m.Key, "content-type", StringComparison.CurrentCultureIgnoreCase));
					if (contentType.Value.Any(m => m.Contains("application/json")))
					{
						terror = JsonSerializer.Deserialize<SlackError>(body, SerializerOptions());
					}
					else if (contentType.Value.Any(m => m.Contains("text/plain")))
					{
						terror = new SlackError() { Message = body };
					}

				}
			}
			catch (Exception ex)
			{
				terror = new SlackError() { Message = ex.Message };
			}

			//done
			throw new SlackServiceException($"Problem with Slack Request - {response.StatusCode}", terror)
			{
				Response = response
			};

		}


		protected string Serialize<T>(T model)
		{
			return JsonSerializer.Serialize(model, new JsonSerializerOptions()
			{
				WriteIndented = true,
				IgnoreNullValues = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			});
		}

		protected string Serialize(object model)
		{
			return Serialize<object>(model);
		}



		protected JsonSerializerOptions SerializerOptions()
		{
			return SerializerHelpers.Options();
		}

		#endregion

	}
}
