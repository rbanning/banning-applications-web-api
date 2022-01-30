using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BanningApplications.WebApi.Helpers
{
    public static class SecureNotificationExtensions
    {
        
		public static Dictionary<string, object> ParseDictionary(this string data)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(data)) { return new Dictionary<string, object>(); }
				return JsonSerializer.Deserialize<Dictionary<string, object>>(data, SerializerHelpers.Options());
				//(old 2.2 version) return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static string SerializeDictionary(this Dictionary<string, object> data)
		{
			try
			{
				if (data == null) { return null; }
				return JsonSerializer.Serialize(data);
				//(old 2.2 version) return Newtonsoft.Json.JsonConvert.SerializeObject(data);
			}
			catch (Exception)
			{
				return null;
			}
		}


    }
}
