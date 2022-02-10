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
	//[Authorize()]
	[Route("api/unsplash/award-winners")]
    public class UnsplashAwardWinnersController: ApiBaseController
    {
	    private readonly IMapper _mapper;
	    private readonly IUnsplashAwardWinnersRepository _repo;

	    public UnsplashAwardWinnersController(
		    IMapper mapper,
		    IUnsplashAwardWinnersRepository repo)
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
		    return new OkObjectResult(_mapper.Map<IEnumerable<UnsplashAwardWinnerDto>>(results));
	    }

	    [HttpGet("categories/{year}")]
	    public async Task<IActionResult> GetAllCategoriesAsync([FromRoute] int year, [FromQuery] bool inclArchived)
	    {
		    return new OkObjectResult( await _repo.GetCategories(year, inclArchived) );
	    }

	    [HttpGet("category/{category}")]
	    public async Task<IActionResult> GetAllByCategoryAsync([FromRoute] string category, [FromQuery] bool inclArchived)
	    {
		    return await GetAllByCategoryAsync(category, null, inclArchived);
	    }
	    [HttpGet("category/{category}/{year}")]
	    public async Task<IActionResult> GetAllByCategoryAsync([FromRoute] string category, [FromRoute] int? year, [FromQuery] bool inclArchived)
	    {
		    var results = await _repo.GetAllByCategory(category, year, inclArchived);
		    return new OkObjectResult(_mapper.Map<IEnumerable<UnsplashAwardWinnerDto>>(results));
	    }

	    [HttpGet("winners")]
	    public async Task<IActionResult> GetAllWinnersAsync([FromQuery] bool inclArchived)
	    {
		    return await GetAllWinnersAsync(null, inclArchived);
	    }
	    [HttpGet("winners/{year}")]
	    public async Task<IActionResult> GetAllWinnersAsync([FromRoute] int? year, [FromQuery] bool inclArchived)
	    {
		    var results = await _repo.GetAllWinners(year, inclArchived);
		    return new OkObjectResult(_mapper.Map<IEnumerable<UnsplashAwardWinnerDto>>(results));
	    }


	    [HttpGet("{id}", Name = "GetUnsplashAwardWinner")]
	    public async Task<IActionResult> GetAsync([FromRoute] string id)
	    {
		    var result = await _repo.GetAsync(id);
		    if (result == null)
		    {
			    return NotFound();
		    }
		    //else
		    return new OkObjectResult(_mapper.Map<UnsplashAwardWinnerDto>(result));
	    }

	    #endregion


	    #region >> SETTERS <<

	    [Authorize(Policy = Policies.Names.AllowAdmin)]
	    [HttpPost()]
	    public async Task<IActionResult> CreateAsync([FromBody] UnsplashAwardWinnerCreateDto model, [FromQuery] bool inclPhotoInResult = true)
	    {
		    var usr = GetAppUser();
		    if (usr == null)
		    {
			    return Unauthorized();
		    }

			//validate
		    if (!(await _repo.PhotoExistsAsync(model.PhotoId)))
		    {
			    return new BadRequestObjectResult("Invalid photo id - could not locate photo with that id");
		    }
		    if (await _repo.AwardWinnerExistsAsync(model.PhotoId, model.Category, model.Year))
		    {
			    return new BadRequestObjectResult(
				    "An award winner record with this information already exists in the database");
		    }

			//create
			var entity = new UnsplashAwardWinner()
			{
				PhotoId = model.PhotoId,
				Category = model.Category,
				Year = model.Year,
				Winner = model.Winner
			};
			entity = await _repo.CreateAsync(entity, usr.Email);
			await _repo.SaveAsync();

			if (inclPhotoInResult)
			{
				entity = await _repo.GetAsync(entity.Id);	//adds the photo to the record
			}


			return CreatedAtRoute("GetUnsplashAwardWinner", new { id = entity.Id}, _mapper.Map<UnsplashAwardWinnerDto>(entity));
	    }


		
		[Authorize(Policy = Policies.Names.AllowAdmin)]
	    [HttpPatch("{id}")]
	    public async Task<IActionResult> PatchAsync([FromRoute] string id, [FromBody] PatchDto patch, [FromQuery] bool inclPhotoInResult = true)
	    {
		    var patches = new List<PatchDto>()
		    {
			    patch
		    };

		    return await PatchAsync(id, patches, inclPhotoInResult);
	    }	
	    

		[Authorize(Policy = Policies.Names.AllowAdmin)]
	    [HttpPatch("{id}/multiple")]
	    public async Task<IActionResult> PatchAsync([FromRoute] string id, [FromBody] List<PatchDto> patches, [FromQuery] bool inclPhotoInResult = true)
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

		    if (inclPhotoInResult)
		    {
			    entity = await _repo.GetAsync(entity.Id);	//adds the photo to the record
		    }

		    return new OkObjectResult(_mapper.Map<UnsplashAwardWinnerDto>(entity));
	    }	

	    #endregion
    }
}
