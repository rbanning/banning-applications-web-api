using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Services.Google
{
	public interface IGooglePlaces
	{
		string GenerateSession();
		Task<GooglePlacesResponseMessage> Lookup(string query, string session);
		Task<GooglePlacesResponseMessage> GetPlace(string place_id, string session);
	}

    public class GooglePlaces: IGooglePlaces
    {
		private const string api_key = "AIzaSyB5Hy8Sx2cFl7bUbP6GBIk-Zg8Y0k7WfY0";
		private readonly IHttpClientFactory _clientFactory;

		public GooglePlaces(
			IHttpClientFactory clientFactory
			)
		{
			_clientFactory = clientFactory;
		}

		public string GenerateSession()
		{
			return Guid.NewGuid().ToString();
		}

		public async Task<GooglePlacesResponseMessage> Lookup(string query, string session)
		{
			string url = BuildUrl("autocomplete", session) +
							$"&types=address&input={query}";
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			var client = _clientFactory.CreateClient("maps_google");
			var response = await client.SendAsync(request);

			return new GooglePlacesResponseMessage(response);

		}

		public async Task<GooglePlacesResponseMessage> GetPlace(string place_id, string session)
		{
			string fields = "address_component,formatted_address,name,geometry,url,type";
			string url = BuildUrl("details", session) +
							$"&fields={fields}&place_id={place_id}";
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			var client = _clientFactory.CreateClient("maps_google");
			var response = await client.SendAsync(request);

			return new GooglePlacesResponseMessage(response);

		}


		private string BuildUrl(string path, string session)
		{
			return $"place/{path}/json?key={api_key}&sessiontoken={session}";
		}
    }

	//from https://stackoverflow.com/a/54187518/2113712

	public class GooglePlacesResponseMessage: IActionResult
	{
		private readonly HttpResponseMessage _responseMessage;

		public GooglePlacesResponseMessage(HttpResponseMessage responseMessage)
		{
			_responseMessage = responseMessage;
		}

		public async Task ExecuteResultAsync(ActionContext context)
		{
			var response = context.HttpContext.Response;

			if (_responseMessage == null)
			{
				throw new InvalidOperationException("GooglePlacesResponseMessage - message cannot be null");
			}

			using (_responseMessage)
			{
				// -- STATUS --
				response.StatusCode = (int)_responseMessage.StatusCode;

				// -- REASON --
				var feature = context.HttpContext.Features.Get<IHttpResponseFeature>();
				if (feature != null)
				{
					feature.ReasonPhrase = _responseMessage.ReasonPhrase;
				}


				// -- HEADERS ---
				var headers = _responseMessage.Headers;

				// Ignore the Transfer-Encoding header if it is just "chunked".
				// We let the host decide about whether the response should be chunked or not.
				if (headers.TransferEncodingChunked == true &&
					headers.TransferEncoding.Count == 1)
				{
					headers.TransferEncoding.Clear();
				}

				foreach (var header in headers)
				{
					response.Headers.Append(header.Key, header.Value.ToArray());
				}


				// -- CONTENT --
				if (_responseMessage.Content != null)
				{
					var contentHeaders = _responseMessage.Content.Headers;

					// Copy the response content headers only after ensuring they are complete.
					// We ask for Content-Length first because HttpContent lazily computes this
					// and only afterwards writes the value into the content headers.
					var unused = contentHeaders.ContentLength;

					foreach (var header in contentHeaders)
					{
						response.Headers.Append(header.Key, header.Value.ToArray());
					}

					await _responseMessage.Content.CopyToAsync(response.Body);

				}

			}
		}
	}
}
