using System;
using System.Collections.Generic;
using System.Linq;

namespace BanningApplications.WebApi.Helpers
{
	public static class StringListExtensions
	{
		public const string STRING_DELIM = "|";

		public static string ToDelimString(this List<string> me)
		{
			return ToDelimString(me, STRING_DELIM);
		}
		//conjunction is added after the last delim to create a readable list like: a, b, and c
		public static string ToDelimString(this List<string> me, string delim, string conjunction = null)
		{
			if (me == null || me.Count == 0) { return null; }
			if (string.IsNullOrEmpty(delim)) { delim = STRING_DELIM; }


			if (me.Count > 1 && !string.IsNullOrEmpty(conjunction))
			{
				var clone = new List<string>(me);
				clone[^1] = $"{conjunction} {clone.Last()}";	//clone[^1] uses the unary operator (^) to indicate index from the end
				return string.Join(delim, clone);
			}

			//else no need to clone
			return string.Join(delim, me);
		}

		public static List<string> ToListString(this string me)
		{
			return ToListString(me, STRING_DELIM);
		}
		public static List<string> ToListString(this string me, string delim)
		{
			if (me == null) { return null; }
			if (string.IsNullOrEmpty(delim)) { delim = STRING_DELIM; }

			var ret = me.Split(delim);
			return ret.Where(m => !string.IsNullOrEmpty(m)).ToList();
		}

		public static string AddToListString(this string me, string value)
		{
			return AddToListString(me, value, STRING_DELIM);
		}
		public static string AddToListString(this string me, string value, string delim)
		{
			var list = me.ToListString(delim);
			if (list == null) { list = new List<string>(); }

			if (!list.Any(m => string.Equals(m, value, StringComparison.CurrentCultureIgnoreCase)))
			{
				list.Add(value);
			}

			return list.ToDelimString(delim);
		}

		public static string RemoveFromListString(this string me, string value)
		{
			return RemoveFromListString(me, value, STRING_DELIM);
		}
		public static string RemoveFromListString(this string me, string value, string delim)
		{
			var list = me.ToListString(delim);

			if (list != null)
			{
				list = list.Where(m => !string.Equals(m, value, StringComparison.CurrentCultureIgnoreCase)).ToList();
			}

			return list?.ToDelimString(delim);
		}


	}
}
