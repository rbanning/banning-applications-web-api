using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using BanningApplications.WebApi.Helpers;

namespace BanningApplications.WebApi.Dtos.ValueConverters
{

	public class StringToListConverter : IValueConverter<string, List<string>>
	{
		public List<string> Convert(string sourceMember, ResolutionContext context)
		{
			if (string.IsNullOrEmpty(sourceMember)) { return new List<string>(); }

			//else
			return sourceMember.ToListString();
		}
	}

	public class ListToStringConverter : IValueConverter<List<string>, string>
	{
		public string Convert(List<string> sourceMember, ResolutionContext context)
		{
			if (sourceMember == null) { return null; }

			//else
			return sourceMember.ToDelimString();
		}
	}

	public static class StringList
	{
		public static string Add(string existing, string newItem)
		{
			var list = new StringToListConverter().Convert(existing, null);
			if (!list.Any(m => string.Equals(m, newItem, StringComparison.CurrentCultureIgnoreCase)))
			{
				list.Add(newItem);
			}
			return new ListToStringConverter().Convert(list, null);
		}

		public static string Add(string existing, List<string> newItems)
		{
			var list = new StringToListConverter().Convert(existing, null);
			foreach (var item in newItems)
			{
				if (!list.Any(m => string.Equals(m, item, StringComparison.CurrentCultureIgnoreCase)))
				{
					list.Add(item);
				}
			}
			return new ListToStringConverter().Convert(list, null);
		}

		public static string Remove(string existing, string item)
		{
			var list = new StringToListConverter().Convert(existing, null)
					.Where(m => !string.Equals(m, item, StringComparison.CurrentCultureIgnoreCase))
					.ToList();
			return new ListToStringConverter().Convert(list, null);
		}

		public static string Remove(string existing, List<string> items)
		{
			var list = new StringToListConverter().Convert(existing, null)
					.Where(m => !items.Any(i => string.Equals(m, i, StringComparison.CurrentCultureIgnoreCase)))
					.ToList();
			return new ListToStringConverter().Convert(list, null);
		}

		public static string Parse(string item)
		{
			var list = ToList(item);
			return list.Count == 0 ? null : new ListToStringConverter().Convert(list, null);
		}

		public static List<string> ToList(string item)
		{
			if (string.IsNullOrEmpty(item)) { return new List<string>(); }
			else if (item.Contains(Constants.LIST_DELIM))
			{
				return item.Split(Constants.LIST_DELIM)
					.Where(m => !string.IsNullOrEmpty(m))
					.ToList();
			}
			else if (item.Contains(","))
			{
				return item.Split(",")
					.Where(m => !string.IsNullOrEmpty(m))
					.ToList();
			}
			//else
			return new List<string>() { item };
		}

	}


}
