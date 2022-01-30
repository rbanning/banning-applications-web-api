using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BanningApplications.WebApi.Dtos.Slack;
using BanningApplications.WebApi.Services.Slack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanningApplications.WebApi.Controllers.Slack
{
	[Authorize()]   /* User will be authorized by valid scope in each method */
	[Route("api/slack/message")]
	[ApiController]
    public class SlackMessageController: ApiBaseController
    {
	    private readonly ISlackService _slack;

	    public SlackMessageController(
		    ISlackService slack)
	    {
		    _slack = slack;
	    }


	    [HttpPost]
	    public async Task<IActionResult> PostMessage([FromBody] SlackMessagePostDto model)
	    {
			//try to configure slack
		    var usr = GetAppUser();
			_slack.Configure(usr.Scope);

			if (!_slack.IsConfigured)
			{
				return Unauthorized("Service is not supported");
			}

			try
			{
				var result = await _slack.SendMessageAsync(model.Channel, model.Message);
				if (result == null)
				{
					return NotFound("Channel not found");
				}
				//else
				return new OkObjectResult(new { response = result });
			}
			catch (SlackServiceException e)
			{
				return BadRequest(e.Message);
			}

			
	    }
	}
}
