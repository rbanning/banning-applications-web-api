using AutoMapper;

namespace BanningApplications.WebApi.Dtos.unsplash
{
	// ReSharper disable once InconsistentNaming
	public class unsplashMappingProfile: Profile
    {
	    public unsplashMappingProfile()
	    {
		    CreateMap<Entities.unsplash.UnsplashPhotographer, UnsplashPhotographerDto>();
	    }
    }
}
