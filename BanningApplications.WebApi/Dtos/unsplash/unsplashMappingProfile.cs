using AutoMapper;
using BanningApplications.WebApi.Helpers;

namespace BanningApplications.WebApi.Dtos.unsplash
{
	// ReSharper disable once InconsistentNaming
	public class unsplashMappingProfile: Profile
    {
	    public unsplashMappingProfile()
	    {
		    CreateMap<Entities.unsplash.UnsplashPhotographer, UnsplashPhotographerDto>();

		    CreateMap<Entities.unsplash.UnsplashPhoto, UnsplashPhotoDto>()
			    .ForMember(m => m.Tags, opt => opt.MapFrom(src => src.TagsJson.ToListString()))
			    .ForMember(m => m.Topics, opt => opt.MapFrom(src => src.TopicsJson.ToListString()))
			    .ForMember(m => m.Location, opt => opt.MapFrom(src => src.Location.Deserialize<UnsplashLocationDto>()))			    
			    .ForMember(m => m.Urls, opt => opt.MapFrom(src => UnsplashPhotoUrlsDto.DeserializeFrom(src.UrlsJson)));

		    CreateMap<Entities.unsplash.UnsplashAwardWinner, UnsplashAwardWinnerDto>();

		    CreateMap<Entities.unsplash.GameScore, GameScoreDto>();
	    }
    }
}
