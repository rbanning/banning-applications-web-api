using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace BanningApplications.WebApi.Helpers
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, object> Set(this Dictionary<string, object> dict, string key, object value)
		{
			if (dict == null) { throw new ArgumentNullException(nameof(dict)); }
			if (dict.Keys.Contains(key))
			{
				dict[key] = value;
			} else
			{
				dict.Add(key, value);
			}
			return dict;
		}

        public static dynamic ToDynamic<TValue>(this Dictionary<string, TValue> dict)
        {
	        return dict.Aggregate(
		        (new ExpandoObject() as IDictionary<string, object>),
		        (expando, current) =>
		        {
			        if (!expando.ContainsKey(current.Key))
			        {
				        expando.Add(current.Key, current.Value);
			        }
					return expando;
		        });
        }

        public static Dictionary<string, string> ToStringDictionary(this Hashtable hashtable)
        {
			return hashtable?.Cast<DictionaryEntry>()
				.ToDictionary(d => d.Key.ToString(), d => d.Value?.ToString());
        }
    }
}
