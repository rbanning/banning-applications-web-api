using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

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


		//FROM: https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
		public static bool IsValidEmail(this string email)
		{
			if (string.IsNullOrWhiteSpace(email))
			{
				return false;
			}

			try
			{
				// Normalize the domain
				email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
					RegexOptions.None, TimeSpan.FromMilliseconds(200));

				// Examines the domain part of the email and normalizes it.
				string DomainMapper(Match match)
				{
					// Use IdnMapping class to convert Unicode domain names.
					var idn = new IdnMapping();

					// Pull out and process domain name (throws ArgumentException on invalid)
					string domainName = idn.GetAscii(match.Groups[2].Value);

					return match.Groups[1].Value + domainName;
				}
			}
			catch (RegexMatchTimeoutException e)
			{
				return false;
			}
			catch (ArgumentException e)
			{
				return false;
			}

			try
			{
				return Regex.IsMatch(email,
					@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
					RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
			}
			catch (RegexMatchTimeoutException)
			{
				return false;
			}
		}
	}
}
