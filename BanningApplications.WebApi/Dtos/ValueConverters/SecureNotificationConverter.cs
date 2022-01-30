using System.Collections.Generic;
using AutoMapper;
using BanningApplications.WebApi.Helpers;

namespace BanningApplications.WebApi.Dtos.ValueConverters
{
	public class MessageStringToDictionaryConverter: IValueConverter<string, Dictionary<string, object>>
	{
		public Dictionary<string, object> Convert(string sourceMember, ResolutionContext context)
		{
			sourceMember ??= string.Empty;
			return sourceMember.ParseDictionary();
		}
	}


}
