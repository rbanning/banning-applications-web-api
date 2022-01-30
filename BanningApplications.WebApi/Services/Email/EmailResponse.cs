using System.Text.Json.Serialization;

namespace BanningApplications.WebApi.Services.Email
{
    public class EmailResponse
    {
		public string Message { get; set; }
		public int StatusCode { get; set; }
		public string ErrorMessage { get; set; }
		public bool IsError => !string.IsNullOrEmpty(ErrorMessage) || StatusCode >= 400;
		public bool Success => !IsError;
		public string Body { get; set; }

		[JsonIgnore]
		public object RawResponse { get; set; }



    }
}
