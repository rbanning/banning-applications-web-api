using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanningApplications.WebApi.Dtos;
using BanningApplications.WebApi.Services.Trello;
using BanningApplications.WebApi.Services.Trello.Models;
using BanningApplications.WebApi.Services.Trello.Models.TrelloDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanningApplications.WebApi.Controllers.Trello.Butler
{

	[Route("api/trello/butler/update-cards")]
	[AllowAnonymous]    //see note below for authentication
	[ApiController]
	public class TrelloButlerUpdateCardController: TrelloButlerBaseController
    {

	    public TrelloButlerUpdateCardController(ITrelloService service)
			:base(service)
	    { }

	    #region >> Update Card's Cover (color or sticker)


	    //Update Cover based on Label Color
	    [HttpPost("{id}/cover-from-label")]
	    public async Task<IActionResult> UpdateCoverFromLabelColor([FromRoute] string id, [FromBody] TrelloLabel model, [FromQuery] string condition, [FromQuery] string value)
	    {
		    var MESSAGE = "update cover";

		    //authenticate and configure the TrelloService
		    if (!AutoConfigureTrelloServiceIfNeeded())
		    {
			    return Unauthorized();
		    }

		    if (!TrelloCardCover.IsValidColor(model.Color))
		    {
			    return BadRequest("Invalid/missing label color");
		    }

		    try
		    {
			    var card = await _trelloService.GetCardAsync(id);
			    if (card == null)
			    {
				    return NotFound();
			    }

			    PatchDto patch = null;

				//check condition
				var filter = BuildLabelFilter(condition, value) ?? (label => true);	//default to true;
				if (filter(model))
				{
					patch = new PatchDto(PatchOperation.replace, "/cover", model.Color);
				}

			    if (patch == null)
			    {
				    return OkResult(MESSAGE, card, "Condition resulted in no updates");
			    }
			    
			    
			    //else
			    {
				    var result = await _trelloService.PatchCardAsync(id, patch);
				    return OkResult(MESSAGE, result, patch);

				}
			}
		    catch (TrelloServiceException e)
		    {
			    return new BadRequestObjectResult(e.Message);
		    }
		}

	    //Update Cover based on Label Color of existing Label (that matches condition)
	    [HttpPost("{id}/cover-from-cards-label")]
	    public async Task<IActionResult> UpdateCoverFromLabelCondition([FromRoute] string id, [FromQuery] string condition, [FromQuery] string value)
	    {
		    var MESSAGE = "update cover";

		    //authenticate and configure the TrelloService
		    if (!AutoConfigureTrelloServiceIfNeeded())
		    {
			    return Unauthorized();
		    }


		    try
		    {
			    var card = await _trelloService.GetCardAsync(id);
			    if (card == null)
			    {
				    return NotFound();
			    }

			    PatchDto patch;
				
			    //check condition
			    var filters = new List<Func<TrelloLabel, bool>>()
			    {
				    BuildLabelFilter(condition, value) ?? (label => false), //default to false
				    (label) => !string.IsNullOrEmpty(label.Color)
			    };

				var first = card.Labels.FirstOrDefault(CombineFunc(filters));

				if (first != null)
				{
					patch = new PatchDto(PatchOperation.replace, "/cover", first.Color);
				}
				else
				{
					patch = new PatchDto(PatchOperation.remove, "/cover", null);
				}

			    
			    //else
			    {
				    var result = await _trelloService.PatchCardAsync(id, patch);
				    return OkResult(MESSAGE, result, patch);

				}
			}
		    catch (TrelloServiceException e)
		    {
			    return new BadRequestObjectResult(e.Message);
		    }
		}


		//todo: add ability to create/delete sticker on card

	    #endregion


	    #region >> Update Card's Name <<

	    [HttpPost("{id}/update-name")]
	    public async Task<IActionResult> UpdateListName([FromRoute] string id, [FromBody] TrelloNameUpdateDto dto)
	    {
		    // ReSharper disable once InconsistentNaming
		    var MESSAGE = "update name";
		   
		    //authenticate and configure the TrelloService
		    if (!AutoConfigureTrelloServiceIfNeeded())
		    {
			    return Unauthorized();
		    }

		    try
		    {
			    var card = await _trelloService.GetCardAsync(id);
			    if (card == null)
			    {
				    return NotFound();
			    }

			    PatchDto patch = new PatchDto()
			    {
				    Op = PatchOperation.replace,
				    Path = "/name",
				    Value = dto.GetDynamicName()
			    };
			    MESSAGE += $" dynamic param: {dto.DynamicParam}";

			    var result = await _trelloService.PatchCardAsync(card.Id, patch);
			    return OkResult(MESSAGE, result, patch);
		    }
		    catch (Exception e)
		    {
			    return new BadRequestObjectResult(e.Message);
		    }
	    }

	    #endregion


	    #region >> Move Card(s) Based On Date <<

	    [HttpPost("{id}/move-to-list-by-date")]
	    public async Task<IActionResult> MoveCardBasedOnDate([FromRoute] string id,
		    [FromBody] TrelloMoveCardBasedOnDateDto dto, [FromQuery] int timezone = 0)
	    {
		    // ReSharper disable once InconsistentNaming
		    var MESSAGE = "move card based on date";
		   
		    //authenticate and configure the TrelloService
		    if (!AutoConfigureTrelloServiceIfNeeded())
		    {
			    return Unauthorized();
		    }

		    try
		    {
			    var card = await _trelloService.GetCardAsync(id);
			    if (card == null)
			    {
				    return NotFound();
			    }

			    var advancedService = new TrelloServiceAdvanced(_trelloService);

			    var result = await advancedService.MoveCardBasedOnDate(card, dto.TargetListId, dto.Min, dto.Max,
				    dto.UseStartIfExists, dto.Pos.ToString(), timezone);
			    return OkResult(MESSAGE, result, result == null ? "no change" : "moved card");
		    }
		    catch (Exception e)
		    {
			    return new BadRequestObjectResult(e.Message);
		    }
	    }

	    [HttpPost("{id}/move-to-list-by-date/multiple")]
	    public async Task<IActionResult> MoveCardBasedOnDate([FromRoute] string id,
		    [FromBody] List<TrelloMoveCardBasedOnDateDto> dtos, [FromQuery] int timezone = 0)
	    {
		    // ReSharper disable once InconsistentNaming
		    var MESSAGE = "move card based on date";
		   
		    //authenticate and configure the TrelloService
		    if (!AutoConfigureTrelloServiceIfNeeded())
		    {
			    return Unauthorized();
		    }

		    try
		    {
			    var card = await _trelloService.GetCardAsync(id);
			    if (card == null)
			    {
				    return NotFound();
			    }

			    var advancedService = new TrelloServiceAdvanced(_trelloService);
			    var results = new List<TrelloCard>();
			    foreach (var dto in dtos)
			    {
				    results.Add(await advancedService.MoveCardBasedOnDate(card, dto.TargetListId, dto.Min, dto.Max,
					    dto.UseStartIfExists, dto.Pos.ToString(), timezone));
			    }

			    results = results.Where(m => m != null).ToList();

			    return OkResult(MESSAGE, results, results.Any() ? "card moved" : "no change");
		    }
		    catch (Exception e)
		    {
			    return new BadRequestObjectResult(e.Message);
		    }
	    }


	    [HttpPost("on-board/{id}/move-to-list-by-date")]
	    public async Task<IActionResult> MoveCardsBasedOnDate([FromRoute] string id,
		    [FromBody] TrelloMoveCardBasedOnDateDto dto, [FromQuery] int timezone = 0)
	    {
		    // ReSharper disable once InconsistentNaming
		    var MESSAGE = "move cards on board based on date";
		   
		    //authenticate and configure the TrelloService
		    if (!AutoConfigureTrelloServiceIfNeeded())
		    {
			    return Unauthorized();
		    }

		    try
		    {
			    var cards = await _trelloService.GetBoardCardsAsync(id);
			    if (cards == null)
			    {
				    return NotFound();
			    }

			    var advancedService = new TrelloServiceAdvanced(_trelloService);
			    var result = new List<TrelloCard>();
			    foreach (var card in cards)
			    {
				    result.Add(await advancedService.MoveCardBasedOnDate(card, dto.TargetListId, dto.Min, dto.Max,
					    dto.UseStartIfExists, dto.Pos.ToString(), timezone));
			    }

				//remove all empties
				result = result.Where(m => m != null).ToList();

				//done
				return OkResult(MESSAGE, result, result.Count == 0 ? "no change" : $"moved {result.Count} card(s)");
		    }
		    catch (Exception e)
		    {
			    return new BadRequestObjectResult(e.Message);
		    }
	    }


	    [HttpPost("on-board/{id}/move-to-list-by-date/multiple")]
	    public async Task<IActionResult> MoveCardsBasedOnDate([FromRoute] string id,
		    [FromBody] List<TrelloMoveCardBasedOnDateDto> dtos, [FromQuery] int timezone = 0)
	    {
		    // ReSharper disable once InconsistentNaming
		    var MESSAGE = "move cards on board based on date";
		   
		    //authenticate and configure the TrelloService
		    if (!AutoConfigureTrelloServiceIfNeeded())
		    {
			    return Unauthorized();
		    }

		    try
		    {
			    var cards = await _trelloService.GetBoardCardsAsync(id);
			    if (cards == null)
			    {
				    return NotFound();
			    }

			    var advancedService = new TrelloServiceAdvanced(_trelloService);
			    var result = new List<TrelloCard>();
			    foreach (var card in cards)
			    {
				    foreach (var dto in dtos)
				    {
					    result.Add(await advancedService.MoveCardBasedOnDate(card, dto.TargetListId, dto.Min, dto.Max,
						    dto.UseStartIfExists, dto.Pos.ToString(), timezone));
				    }
			    }

				//remove all empties
				result = result.Where(m => m != null).ToList();

				//done
				return OkResult(MESSAGE, result, result.Count == 0 ? "no change" : $"moved {result.Count} card(s)");
		    }
		    catch (Exception e)
		    {
			    return new BadRequestObjectResult(e.Message);
		    }
	    }



	    #endregion



	    #region >> HELPERS <<

		protected  Func<T, bool> CombineFunc<T>(params Func<T, bool>[] filters)
		{
			if (filters?.Length > 0)
			{
				List<Func<T, bool>> result = filters
					.Where(fn => fn != null)
					.ToList();

				return (label) => result.All(fn => fn(label));
			}

			//else
			return null;
		}
		protected  Func<T, bool> CombineFunc<T>(List<Func<T, bool>> filters)
		{
			if (filters?.Count> 0)
			{
				List<Func<T, bool>> result = filters
					.Where(fn => fn != null)
					.ToList();

				return (label) => result.All(fn => fn(label));
			}

			//else
			return null;
		}

	    protected Func<TrelloLabel, bool> BuildLabelFilter(string condition,
		    string value = null)
	    {
		    Func<TrelloLabel, bool> result = null;

		    switch (condition?.ToLower())
		    {
				//true always
				case "include-all":
					result = (label => true);
					break;

				//false always
				case "exclude-all":
				case "ignore-all":
					result = (label => false);
					break;

			    case "include-by-name":
				    if (!string.IsNullOrEmpty(value))
				    {
					    result = (label => !string.IsNullOrEmpty(label.Name) && label.Name.Contains(value));
				    }
				    break;
			    case "exclude-by-name":
			    case "ignore-by-name":
				    if (!string.IsNullOrEmpty(value))
				    {
					    result = (label => string.IsNullOrEmpty(label.Name) || !label.Name.Contains(value));
				    }
				    break;
		    }


			return result;
	    }



	    #endregion

	    #region >> OkResults <<

	    
	    protected IActionResult OkResult<TResult>(string message, TResult result, string action)
	    {
		    return new OkObjectResult(new {action, message, result});
	    }
	    protected IActionResult OkResult<TResult>(string message, TResult result, PatchDto action)
	    {
		    return new OkObjectResult(new {action, message, result});
	    }

	    protected IActionResult OkResult<TResult>(string message, TResult result, List<PatchDto> action)
	    {
		    return new OkObjectResult(new {action, message, result});
	    }

	    protected IActionResult OkResult<TResult>(string message, IEnumerable<TResult> result, string action)
	    {
		    return new OkObjectResult(new {action, message, result});
	    }
	    protected IActionResult OkResult<TResult>(string message, IEnumerable<TResult> result, PatchDto action)
	    {
		    return new OkObjectResult(new {action, message, result});
	    }

	    protected IActionResult OkResult<TResult>(string message, IEnumerable<TResult> result, List<PatchDto> action)
	    {
		    return new OkObjectResult(new {action, message, result});
	    }

	    #endregion
	}
}
