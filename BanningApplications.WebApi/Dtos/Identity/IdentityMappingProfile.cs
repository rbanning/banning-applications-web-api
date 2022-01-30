using AutoMapper;
using BanningApplications.WebApi.Identity;

namespace BanningApplications.WebApi.Dtos.Identity
{
	public class IdentityMappingProfile : Profile
	{
		public IdentityMappingProfile()
		{
			CreateMap<AppUser, UserDto>()
				.ForMember(m => m.Phone, opt => opt.MapFrom(src => src.PhoneNumberConfirmed ? src.PhoneNumber : null));
			CreateMap<AppUser, UserWithIdDto>()
				.ForMember(m => m.Phone, opt => opt.MapFrom(src => src.PhoneNumberConfirmed ? src.PhoneNumber : null));
		}
	}
}
