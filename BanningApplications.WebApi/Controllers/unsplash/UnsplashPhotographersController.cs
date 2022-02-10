using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BanningApplications.WebApi.Dtos;
using BanningApplications.WebApi.Dtos.unsplash;
using BanningApplications.WebApi.Entities.unsplash;
using BanningApplications.WebApi.Identity;
using BanningApplications.WebApi.Repo.unsplash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanningApplications.WebApi.Controllers.unsplash
{
	[Authorize(Policy = "mock-scope:bannapps")]
    [Route("api/unsplash/photographers")]
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

	    #region >> GETTERS <<

		[Authorize(Policy = "scope:bannapps")]
	    [HttpGet()]
	    public async Task<IActionResult> GetAllAsync([FromQuery] bool inclArchived)
	    {
		    var results = await _repo.GetAllAsync(inclArchived);
		    return new OkObjectResult(_mapper.Map<IEnumerable<UnsplashPhotographerDto>>(results));
	    }

	    [HttpGet("{id}", Name = "GetUnsplashPhotographer")]
	    public async Task<IActionResult> GetAsync([FromRoute] string id)
	    {
		    var result = await _repo.GetAsync(id);
		    if (result == null)
		    {
			    return NotFound();
		    }
			//else
		    return new OkObjectResult(_mapper.Map<UnsplashPhotographerDto>(result));
	    }

	    [HttpGet("username/{username}")]
	    public async Task<IActionResult> GetByUsernameAsync([FromRoute] string username)
	    {
		    var result = await _repo.GetByUsernameAsync(username);
		    if (result == null)
		    {
			    return NotFound();
		    }
			//else
		    return new OkObjectResult(_mapper.Map<UnsplashPhotographerDto>(result));
	    }

	    #endregion

	    #region >> SETTERS <<

		[Authorize(Policy = Policies.Names.AllowAdmin)]
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

		    return CreatedAtRoute("GetUnsplashPhotographer", new {id = entity.Id}, _mapper.Map<UnsplashPhotographerDto>(entity));
	    }

		
		[Authorize(Policy = Policies.Names.AllowAdmin)]
	    [HttpPatch("{id}")]
	    public async Task<IActionResult> PatchAsync([FromRoute] string id, [FromBody] PatchDto patch)
	    {
		    var patches = new List<PatchDto>()
		    {
			    patch
		    };

		    return await PatchAsync(id, patches);
	    }	
	    

		[Authorize(Policy = Policies.Names.AllowAdmin)]
	    [HttpPatch("{id}/multiple")]
	    public async Task<IActionResult> PatchAsync([FromRoute] string id, [FromBody] List<PatchDto> patches)
	    {
		    var usr = GetAppUser();
		    if (usr == null)
		    {
			    return Unauthorized();
		    }

		    var entity = await _repo.GetAsync(id, true);
		    if (entity == null)
		    {
			    return NotFound();
		    }

		    entity = _repo.Patch(entity, patches, usr.Email);
		    await _repo.SaveAsync();
		    return new OkObjectResult(_mapper.Map<UnsplashPhotographerDto>(entity));
	    }	

	    #endregion
    }
}
