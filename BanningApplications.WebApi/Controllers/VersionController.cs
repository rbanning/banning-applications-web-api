using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BanningApplications.WebApi.Controllers
{
	[Route("api/_version")]
	[AllowAnonymous]
    public class VersionController: ApiBaseController
    {
	    [HttpGet]
	    public IActionResult Version()
	    {
		    return new OkObjectResult(new
		    {
			    core = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
			    platform = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
		    });
	    }
    }
}
