using BanningApplications.WebApi.Controllers.Trello;
using BanningApplications.WebApi.Services.Trello;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanningApplications.WebApi.Controllers.hallpass
{
    [Route("api/hallpass/tests/trello")]
    [ApiController]
    [Authorize(Policy = "scope:hallpass")]
    public class HallpassTestsTrelloController: TrelloBaseController
    {

	    public HallpassTestsTrelloController(ITrelloService trelloService)
			:base(trelloService)
	    {
		    _trelloService.Configure(TrelloConfig.Hallpass());
	    }


	}
}
