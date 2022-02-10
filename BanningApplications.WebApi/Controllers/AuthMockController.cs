using BanningApplications.WebApi.Identity;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using BanningApplications.WebApi.Helpers;

namespace BanningApplications.WebApi.Controllers
{
	[Route("api/auth-mock")]
	[ApiController]
    public class AuthMockController: ApiBaseController
    {
	    private readonly IMapper _mapper;
	    private readonly IConfiguration _config;

	    public AuthMockController(IMapper mapper, 
		    IConfiguration config)
	    {
		    _mapper = mapper;
		    _config = config;
	    }


	    [AllowAnonymous]
	    [HttpGet]
	    public IActionResult Test([FromQuery] string scopeId)
	    {
		    var user = MockAuth.CreateAppUser("mock@domain.com", scopeId);
		    var extracted = MockAuth.ExtractScopeFromObfuscateScopeId(user?.Scope);

		    return new OkObjectResult(new
		    {
			    scopeId,
			    user,
			    extracted,
			    check = string.Equals(scopeId, extracted)
		    });

	    }

	    #region >> AUTHENTICATION <<

	    [AllowAnonymous]
		[HttpPost]
		public IActionResult CreateToken([FromBody] Dtos.Identity.LoginDto model)
		{
			if (ModelState.IsValid)
			{
				//todo: how to prevent anyone from using the mock (check password?)
				if (model.Email.IsValidEmail() && RegisteredScopes.IsValid(model.Scope))
				{
					var user = MockAuth.CreateAppUser(model.Email, model.Scope);
					return new OkObjectResult(TokenResult(user));
				}

				//else (generic error message)
				return new BadRequestObjectResult(new AuthController.AuthLoginFailed(AuthController.AuthLoginFailed.FailReason.invalid));
			}

			return BadRequest(ModelState);
		}

		[Authorize]
		[HttpPost("refresh")]
		public IActionResult RefreshToken()
		{
			var who = GetAppUser();
			if (who != null)
			{
				var user = MockAuth.CreateAppUser(who.Email, who.Scope);
				if (user != null && string.Equals(who.SecurityStamp, user.SecurityStamp))
				{
					return new OkObjectResult(TokenResult(user));
				}

			}

			//else
			return new BadRequestObjectResult("Token has expired");
		}


		
		private object TokenResult(AppUser user)
		{
			return new
			{
				token = user.ToAuthToken(_config),
				expires = _config.AuthExpires(),
				user = _mapper.Map<Dtos.Identity.UserDto>(user)
			};
		}

	    #endregion
    }
}
