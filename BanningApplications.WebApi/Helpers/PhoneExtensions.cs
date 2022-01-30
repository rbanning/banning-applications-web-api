using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Helpers
{
    public static class PhoneExtensions
    {
        public static string CleanPhoneData(this string phone)
		{
			phone = phone == null ? null : phone.Trim();
			if (string.IsNullOrEmpty(phone)) { return null; }

			//todo: implement fn to create consistent phone numbers

			return phone;
		}
    }
}
