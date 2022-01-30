using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanningApplications.WebApi.Services.WorldTime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace BanningApplications.WebApi.Controllers.WorldTime
{
	[Route("api/world-time")]
	[AllowAnonymous]
	[ApiController]
	public class WorldTimeController: ApiBaseController
    {
	    private readonly List<string> _authApi = new List<string>
	    {
		    "999c77e7-760d-4d99-be71-88d7d5136b9c",
		    "2ec1f995-e021-4c83-990d-5d602da6a752",
		    "55e3c9a9-270b-4d9c-bc3a-92a7cd64e3d0",
		    "af688405-5cfe-42bd-a095-b248dab10dfb",
		    "766c9521-c7e0-4c79-999f-02c95e76f68d",
		    "cf52f5fb-35e5-4dcb-985a-8ea52f6f1141"
		};
        private readonly IWorldTimeService _service;

        public WorldTimeController(IWorldTimeService service)
        {
	        _service = service;
        }


        [HttpGet("coordinate")]
        public async Task<IActionResult> GetByCoordinates([FromQuery] float latitude, [FromQuery] float longitude)
        {
	        if (!AuthenticatePublicApi())
	        {
		        return Unauthorized();
	        }

	        try
	        {
		        var result = await _service.CurrentByCoordinates(latitude, longitude);
		        return new OkObjectResult(result);
	        }
	        catch (Exception e)
	        {
		        return new BadRequestObjectResult(e);
	        }
        }


        protected bool AuthenticatePublicApi()
        {
	        var auth = Request.Headers.FirstOrDefault(m => string.Equals(m.Key,"X-Hallpass-Api", StringComparison.CurrentCultureIgnoreCase)).Value;

			return !(StringValues.IsNullOrEmpty(auth)) && _authApi.Exists(m => string.Equals(m, auth.First(), StringComparison.CurrentCultureIgnoreCase));

		}
	}
}
