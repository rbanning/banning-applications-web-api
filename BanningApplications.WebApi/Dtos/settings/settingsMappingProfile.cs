using AutoMapper;

namespace BanningApplications.WebApi.Dtos.settings
{
	// ReSharper disable once InconsistentNaming
	public class settingsMappingProfile: Profile
    {
		public settingsMappingProfile()
		{
			CreateMap<Entities.settings.UserSettings, UserSettingsDto>()
				.ForMember(m => m.UserId, opt => opt.MapFrom(src => src.UserId.Substring(src.UserId.Length - 6)))
				.ForMember(m => m.Scope, opt => opt.MapFrom(src => src.Scope.Substring(src.Scope.Length - 6)));
			
		}
    }
}
