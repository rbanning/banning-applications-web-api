using System;
using System.Text.Json;

namespace BanningApplications.WebApi.Dtos.hook
{
    public class HookRequestDto
    {
		public string Id { get; set; }
		public string Path { get; set; }
		public string Headers { get; set; }
		public string Body { get; set; }

		public dynamic BodyObject { get; set; }

		public DateTime CreatedDate { get; set; }

		public static dynamic BodyToObject(string json)
		{
			try
			{
				if (string.IsNullOrEmpty(json)) { return null; }
				dynamic result = JsonSerializer.Deserialize<dynamic>(json, Helpers.SerializerHelpers.Options());
				//(old 2.2 version) dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
				return result;
			}
			catch (Exception ex)
			{
				return ex;
			}
		}

	}
}
