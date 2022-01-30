using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BanningApplications.WebApi.Dtos;
using BanningApplications.WebApi.Services.Trello;
using BanningApplications.WebApi.Services.Trello.Models.TrelloDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanningApplications.WebApi.Controllers.Trello.Butler
{
	[Route("api/trello/butler/update-lists")]
	[AllowAnonymous]    //see note below for authentication
	[ApiController]
    public class TrelloButlerUpdateListController: TrelloButlerBaseController
    {
	    public TrelloButlerUpdateListController(ITrelloService service)
		    :base(service)
	    { }


	    #region >> Update List Name <<

	    [HttpPost("{id}/update-name")]
	    public async Task<IActionResult> UpdateListName([FromRoute] string id, [FromBody] TrelloNameUpdateDto dto)
	    {
		    var MESSAGE = "update name";
		   
		    //authenticate and configure the TrelloService
		    if (!AutoConfigureTrelloServiceIfNeeded())
		    {
			    return Unauthorized();
		    }

		    try
		    {
			    var list = await _trelloService.GetListAsync(id);
			    if (list == null)
			    {
				    return NotFound();
			    }

			    PatchDto patch = new PatchDto()
			    {
					Op = PatchOperation.replace,
					Path = "/name",
					Value = dto.GetDynamicName()
			    };

			    var result = await _trelloService.PatchListAsync(list.Id, patch);
			    return OkResult(MESSAGE, result, patch);
		    }
		    catch (Exception e)
		    {
			    return new BadRequestObjectResult(e.Message);
		    }
	    }

	    #endregion


	    #region >> OkResult <<

	    
	    protected IActionResult OkResult<TResult>(string message, TResult result, string action = null)
	    {
		    return new OkObjectResult(new {action, message, result});
	    }
	    protected IActionResult OkResult<TResult>(string message, TResult result, PatchDto action = null)
	    {
		    return new OkObjectResult(new {action, message, result});
	    }

	    protected IActionResult OkResult<TResult>(string message, TResult result, List<PatchDto> action = null)
	    {
		    return new OkObjectResult(new {action, message, result});
	    }


	    #endregion
    }
}
