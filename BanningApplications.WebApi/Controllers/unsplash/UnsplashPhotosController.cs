using System;
using System.Threading.Tasks;
using AutoMapper;
using BanningApplications.WebApi.Dtos.unsplash;
using BanningApplications.WebApi.Entities.unsplash;
using BanningApplications.WebApi.Helpers;
using BanningApplications.WebApi.Repo.unsplash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanningApplications.WebApi.Controllers.unsplash
{
	[Authorize(Policy = "scope:bannapps")]
	[Route("api/unsplash/photos")]
	[ApiController]
    public class UnsplashPhotosController
    {
	    private readonly IMapper _mapper;
	    private readonly IUnsplashPhotographersRepository _photographerRepo;

	    public UnsplashPhotosController(
		    IMapper mapper,
		    IUnsplashPhotographersRepository photographerRepo)
	    {
		    _mapper = mapper;
		    _photographerRepo = photographerRepo;
	    }


	    [HttpPost("test")]
	    public async Task<IActionResult> CreateAsync([FromBody] UnsplashPhotoCreateDto model)
	    {
		    try
		    {
			    var result = new UnsplashPhoto()
			    {
				    UserName = model.UserName,
				    Width = model.Width,
				    Height = model.Height,
				    Published = model.Published,
				    BlurHash = model.BlurHash,
				    Description = model.Description,
				    AltDescription = model.AltDescription,
				    Color = model.Color,
				    Location = model.Location.Serialize(),
				    TagsJson = model.Tags.ToDelimString(),
				    TopicsJson = model.Topics.ToDelimString(),
				    UrlsJson = model.Urls.Serialize()
			    };
			    result.Photographer = await _photographerRepo.GetByUsernameAsync(result.UserName);

			    var mapped = _mapper.Map<UnsplashPhotoDto>(result);
				return new OkObjectResult(new { result, mapped });

			}
			catch (Exception e)
		    {
			    return new BadRequestObjectResult(e.Message);
		    }
	    }	    
	    
	    [HttpPost("test-photo-url")]
	    public IActionResult Test([FromBody] string json)
	    {
		    try
		    {
			    var result = UnsplashPhotoUrlsDto.DeserializeFrom(json);
			    return new OkObjectResult(result);
		    }
		    catch (Exception e)
		    {
			    return new BadRequestObjectResult(e.Message);
		    }
	    }
    }
}
