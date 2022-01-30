using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BanningApplications.WebApi.Controllers.hallpass
{
	[Route("api/hallpass/tests/scope")]
	[ApiController]
	[Authorize]
	public class HallpassTestsScopeController: ApiBaseController
    {
	    private readonly ILogger _logger;
	    private readonly IMapper _mapper;

	    public HallpassTestsScopeController(
		    ILogger<HallpassTestsScopeController> logger,
		    IMapper mapper
		    )
			:base()
	    {
		    _logger = logger;
		    _mapper = mapper;
	    }


	    #region >> Scopes <<

	    [HttpGet]
	    public IActionResult GetMyScope()
	    {
		    var usr = GetAppUser();
		    if (usr == null)
		    {
			    return BadRequest();
		    }

		    var scope = Identity.RegisteredScopes.Find(usr.Scope);
		    return new OkObjectResult(new
		    {
			    usr = _mapper.Map<Dtos.Identity.UserDto>(usr),
			    scope
		    });
	    }

	    [HttpGet("list")]
	    [Authorize(Policy = Identity.Policies.Names.AllowAdmin)]
		[Authorize(Policy = "scope:hallpass")]
	    public IActionResult GetAllScopes()
	    {
		    return new OkObjectResult(Identity.RegisteredScopes.All());
	    }

	    #endregion

		#region >> DynamicIdentity <<

		[HttpGet("dynamic-identity")]
	    public IActionResult DynamicIdentityTest()
	    {
		    try
		    {
			    var headers = base.DynamicIdentityHeaders();
			    var identity = base.GetDynamicIdentity();	

				//log the request
			    if (identity != null && identity.IsValid)
			    {
				    _logger.LogInformation($"Dynamic Identity: SUCCESS - {identity.Scope.Name} - {identity.User}");
			    }
			    else
			    {
				    var scopeKey = string.IsNullOrEmpty(headers["SCOPE-CODE"])
					    ? (string.IsNullOrEmpty(headers["SCOPE"]) ? "??" : headers["SCOPE"])
					    : headers["SCOPE-CODE"];
				    _logger.LogWarning($"Dynamic Identity: FAILED - attempted scope: {scopeKey}");
			    }
				return new OkObjectResult(new { headers, identity });
		    }
			catch (Exception e)
		    {
			    _logger.LogError("Error getting dynamic identity", e);
			    return BadRequest();
		    }
	    }

	    
	    #endregion

	}
}
