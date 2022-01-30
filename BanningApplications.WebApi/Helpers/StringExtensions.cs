using System;
using System.Linq;

namespace BanningApplications.WebApi.Helpers
{
    public static class StringExtensions
    {
        public static string ToBase64(this string text)
		{
			if (string.IsNullOrEmpty(text)) { return string.Empty; }

			var bytes = System.Text.Encoding.UTF8.GetBytes(text);
			return Convert.ToBase64String(bytes);
		}

        public static string FromBase64(this string text64)
		{
			if (string.IsNullOrEmpty(text64)) { return string.Empty; }

			var bytes = System.Convert.FromBase64String(text64);
			return System.Text.Encoding.UTF8.GetString(bytes);
		}

		public static string RandomString(int length)
		{
			var chars = "abcdefghijklmnopqrstuvwzyz0123456789".Split("");
			var ret = "";
			var rand = new Random();

			while (ret.Length < length)
			{
				var ch = chars[rand.Next(0, chars.Length)];
				ret += rand.Next(0,2) == 0 ? ch.ToUpper() : ch;
			}

			return ret;
		}

		public static string ConvertToBlobSegment(this string str)
		{
			return new string(str.ToCharArray()
					.Select(ConvertToBlobChar)
					.ToArray())
				.ToLower();
		}

		public static char ConvertToBlobChar(this char ch)
		{
			return char.IsLetterOrDigit(ch) ? ch : '-';
		}

	}
}
