using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BanningApplications.WebApi.Helpers;

namespace BanningApplications.WebApi.Services.Trello.Models.TrelloDtos
{
    public class TrelloNameUpdateDto
    {
        [Required] public string Name { get; set; }

        public bool IsDynamic { get; set; }

        public string DynamicParam { get; set; }

        public TrelloNameUpdateDto()
        {
	        IsDynamic = false;
        }

        public string GetDynamicName()
        {
	        var name = Name;

	        if (IsDynamic)
	        {
		        var dic = DynamicReplacements();
                foreach (var key in dic.Keys)
                {
	                if (name.Contains(key, StringComparison.CurrentCultureIgnoreCase))
	                {
		                name = name.Replace(key, dic[key](DynamicParam ?? ""), StringComparison.CurrentCultureIgnoreCase);
	                }
                }
	        }

            return name;
        }

        protected Dictionary<string, Func<string, string>> DynamicReplacements()
        {
	        return new Dictionary<string, Func<string, string>>()
	        {
                //the following do use an OPTIONAL DynamicParam
		        {"%DATE%", (param => GetDateTime(DynamicParam).ToShortDateString())},
		        {"%DATE_LONG%", (param => GetDateTime(DynamicParam).ToLongDateString())},
		        {"%TIME%", (param => GetDateTime(DynamicParam).ToShortTimeString())},
		        {"%TIME_LONG%", (param => GetDateTime(DynamicParam).ToLongTimeString())},

                //the following use a REQUIRED DynamicParam
                {"%COUNT%", (param => param.Length.ToString()) }
	        };
        }

        protected DateTime GetDateTime(string key = "NOW")
        {
            key ??= "NOW";
            switch (key.ToUpper())
            {
	            case "NOW":
                    return DateTime.Now;
                case "NOW_UTC":
                    return DateTime.UtcNow;
                case "TOMORROW":
                    return DateTime.Now.AddDays(1);
                case "TOMORROW_UTC":
                    return DateTime.UtcNow.AddDays(1);
                case "NEXT_BUSINESS_DAY":
                    return DateTime.Now.AddDays(1).MoveToBusinessDay();
                case "NEXT_BUSINESS_DAY_UTC":
	                return DateTime.UtcNow.AddDays(1).MoveToBusinessDay();
            }

            //else
            return DateTime.MinValue;   //indicates an error
        }
    }
}
