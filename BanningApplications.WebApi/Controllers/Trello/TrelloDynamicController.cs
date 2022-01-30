using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BanningApplications.WebApi.Services.Trello;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanningApplications.WebApi.Controllers.Trello
{
	[Route("api/trello/dynamic")]
	[AllowAnonymous]	//see note below for authentication
	[ApiController]
    public class TrelloDynamicController: TrelloBaseController
    {
	    public TrelloDynamicController(ITrelloService trelloService)
			:base(trelloService)
	    {
			//NOTE Trello Service is configured using the base class's AutoConfigureTrelloServiceIfNeeded()
			//		which is called on all of the base class routes
			//		and should be called on any routes created here.
		}



	}
}
