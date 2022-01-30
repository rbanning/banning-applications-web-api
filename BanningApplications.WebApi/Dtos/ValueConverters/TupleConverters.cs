using AutoMapper;

namespace BanningApplications.WebApi.Dtos.ValueConverters
{
	public class IdValueTupleToValue : IValueConverter<(string id, string value), string>
	{
		public string Convert((string id, string value) sourceMember, ResolutionContext context)
		{
			return sourceMember.value;
		}
	}
}
