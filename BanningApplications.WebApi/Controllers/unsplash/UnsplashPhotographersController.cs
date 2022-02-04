using System.Threading.Tasks;
using AutoMapper;
using BanningApplications.WebApi.Dtos.unsplash;
using BanningApplications.WebApi.Entities.unsplash;
using BanningApplications.WebApi.Repo.unsplash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanningApplications.WebApi.Controllers.unsplash
{
    [Authorize(Policy = "scope:bannapps")]
    [Route("api/unsplash")]
    [ApiController]
    public class UnsplashPhotographersController: ApiBaseController
    {
	    private readonly IMapper _mapper;
	    private readonly IUnsplashPhotographersRepository _repo;

	    public UnsplashPhotographersController(
		    IMapper mapper,
		    IUnsplashPhotographersRepository repo)
	    {
		    _mapper = mapper;
		    _repo = repo;
	    }

	    #region >> SETTERS <<

	    [HttpPost()]
	    public async Task<IActionResult> CreateAsync(UnsplashPhotographerCreateDto model)
	    {
		    var usr = GetAppUser();
		    if (usr == null)
		    {
			    return Unauthorized();
		    }

		    var entity = new UnsplashPhotographer()
		    {
			    UserName = model.UserName,
			    Name = model.Name,
			    Location = model.Location,
			    Bio = model.Bio,
			    Portfolio = model.Portfolio
		    };

		    entity = await _repo.CreateAsync(entity, usr.Email);
		    await _repo.SaveAsync();

		    return new OkObjectResult(_mapper.Map<UnsplashPhotographerDto>(entity));
	    }

	    #endregion
    }
}
