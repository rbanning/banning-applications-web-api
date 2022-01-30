using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.hook
{
    public class hookMappingProfile : Profile
	{
		public hookMappingProfile()
		{
			CreateMap<Entities.hook.HookRequest, HookRequestDto>()
				.ForMember(dest => dest.BodyObject, opt => opt.MapFrom(
					src => HookRequestDto.BodyToObject(src.Body)
				));
		}
	}
}
