using System.Threading.Tasks;
using BanningApplications.WebApi.Services.Trello;
using BanningApplications.WebApi.Services.Trello.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanningApplications.WebApi.Controllers.Trello
{
	[Authorize()]   /* User will be authorized by valid scope in each method */
	[Route("api/trello/authenticated")]
	[ApiController]
    public class TrelloAuthenticatedController: TrelloBaseController
    {

	    public TrelloAuthenticatedController(ITrelloService service)
			:base(service)
	    {}

		// *** Add any additional endpoints here *** //


		// Create Task Card from another Trello Card
		[HttpPost("boards/{id}/create-card-task")]
		public async Task<IActionResult> CreateCardTask([FromRoute] string id, [FromBody] TrelloCardLinkedCreateDto dto)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var advanced = new TrelloServiceAdvanced(_trelloService);
				var result = await advanced.CreateCardLinkedToCard(id, dto);
				return new OkObjectResult(result);

			}
			catch (TrelloServiceAdvanced.TrelloServiceAdvancedException tex)
			{
				switch (tex.ResponseType)
				{
					case TrelloServiceAdvanced.TrelloServiceAdvancedException.ResponseTypeEnum.NotFound:
						return NotFound(tex.Message);
					case TrelloServiceAdvanced.TrelloServiceAdvancedException.ResponseTypeEnum.BadRequest:
						return new BadRequestObjectResult(new {reason = tex.Message});
					case TrelloServiceAdvanced.TrelloServiceAdvancedException.ResponseTypeEnum.ServerError:
						//pass through;
						break;
				}

				throw;
			}
		}



    }

	
}
