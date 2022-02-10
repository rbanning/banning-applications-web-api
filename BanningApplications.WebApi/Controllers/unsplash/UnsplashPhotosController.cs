using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BanningApplications.WebApi.Dtos;
using BanningApplications.WebApi.Dtos.unsplash;
using BanningApplications.WebApi.Entities.unsplash;
using BanningApplications.WebApi.Helpers;
using BanningApplications.WebApi.Identity;
using BanningApplications.WebApi.Repo.unsplash;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanningApplications.WebApi.Controllers.unsplash
{
	[Authorize(Policy = "mock-scope:bannapps")]
	[Route("api/unsplash/photos")]
	[ApiController]
    public class UnsplashPhotosController: ApiBaseController
    {
	    private readonly IMapper _mapper;
	    private readonly IUnsplashPhotographersRepository _photographerRepo;
	    private readonly IUnsplashPhotosRepository _photoRepo;

	    public UnsplashPhotosController(
		    IMapper mapper,
		    IUnsplashPhotographersRepository photographerRepo,
		    IUnsplashPhotosRepository photoRepo)
	    {
		    _mapper = mapper;
		    _photoRepo = photoRepo;
		    _photographerRepo = photographerRepo;
	    }

	    #region >> GETTERS <<

		[Authorize(Policy = "scope:bannapps")]
	    [HttpGet]
	    public async Task<IActionResult> GetAllAsync([FromQuery] bool inclArchived = false)
	    {
		    var result = await _photoRepo.GetAllAsync(inclArchived);
			return new OkObjectResult(_mapper.Map<IEnumerable<UnsplashPhotoDto>>(result));
	    }

	    [HttpGet("photographer/{username}")]
	    public async Task<IActionResult> GetAllByPhotographerAsync([FromRoute] string username, [FromQuery] bool inclArchived = false)
	    {
		    var result = await _photoRepo.GetAllByPhotographerAsync(username, inclArchived);
			return new OkObjectResult(_mapper.Map<IEnumerable<UnsplashPhotoDto>>(result));
	    }

	    [HttpGet("tags/{id}")]
	    public async Task<IActionResult> GetAllByTagAsync([FromRoute] string id, [FromQuery] bool inclArchived = false)
	    {
		    var result = await _photoRepo.GetAllByTagAsync(id, inclArchived);
			return new OkObjectResult(_mapper.Map<IEnumerable<UnsplashPhotoDto>>(result));
	    }

	    [HttpGet("topics/{id}")]
	    public async Task<IActionResult> GetAllByTopicsAsync([FromRoute] string id, [FromQuery] bool inclArchived = false)
	    {
		    var result = await _photoRepo.GetAllByTopicAsync(id, inclArchived);
			return new OkObjectResult(_mapper.Map<IEnumerable<UnsplashPhotoDto>>(result));
	    }


	    [HttpGet("{id}", Name = "GetUnsplashPhoto")]
	    public async Task<IActionResult> GetPhotoAsync([FromRoute] string id)
	    {
		    var result = await _photoRepo.GetAsync(id);
		    if (result == null)
		    {
			    return NotFound();
		    }
			//else
			return new OkObjectResult(_mapper.Map<UnsplashPhotoDto>(result));
	    }

	    #endregion


	    #region >> SETTERS <<

		[Authorize(Policy = Policies.Names.AllowAdmin)]
	    [HttpPost()]
	    public async Task<IActionResult> CreateAsync([FromBody] UnsplashPhotoCreateDto model)
	    {
		    var usr = GetAppUser();
		    if (usr == null)
		    {
			    return Unauthorized();
		    }

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
				    AltDescription = model.AltDescription ?? model.Alt,	//allow client to use either Alt or AltDescription
				    Color = model.Color,
				    Location = model.Location.Serialize(),
				    TagsJson = model.Tags.ToDelimString(),
				    TopicsJson = model.Topics.ToDelimString(),
				    UrlsJson = model.Urls.Serialize()
			    };
			    if (!string.IsNullOrEmpty(model.Id))
			    {
				    result.Id = model.Id;
			    }

			    if (await _photoRepo.ExistsAsync(result.Id))
			    {
				    return new BadRequestObjectResult($"Photo with id = '{result.Id}' already exists in database");
			    }

			    var photographer = await _photographerRepo.GetByUsernameAsync(result.UserName);
			    if (photographer == null)
			    {
				    return new BadRequestObjectResult("Invalid photographer username");
			    }

				//else
				result = await _photoRepo.CreateAsync(result, usr.Email);
				await _photoRepo.SaveAsync();

				result.Photographer = photographer;

				return CreatedAtRoute("GetUnsplashPhoto", new { id = result.Id}, _mapper.Map<UnsplashPhotoDto>(result));

			}
			catch (Exception e)
		    {
			    return new BadRequestObjectResult(e.Message);
		    }
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

		    var entity = await _photoRepo.GetAsync(id, true);
		    if (entity == null)
		    {
			    return NotFound();
		    }

		    entity = _photoRepo.Patch(entity, patches, usr.Email);
		    await _photoRepo.SaveAsync();
		    return new OkObjectResult(_mapper.Map<UnsplashPhotoDto>(entity));
	    }	
	    
		#endregion

    }
}
