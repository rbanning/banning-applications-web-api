using System;
using BanningApplications.WebApi.Services.Trello;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BanningApplications.WebApi.Controllers
{
	[Route("/error")]
	[ApiController]
	public class ErrorController : ControllerBase
	{
		private readonly ILogger _logger;

		public ErrorController(ILogger<EchoController> logger)
		{
			_logger = logger;
		}

		// GET - PRODUCTION ERROR
		[HttpGet()]
		[HttpPost()]
		[HttpPut()]
		[HttpPatch()]
		[HttpDelete()]
		public IActionResult Error()
		{
			return ProcessError(false);
		}

		// GET - DEVELOPMENT ERROR
		[HttpGet("dev")]
		[HttpPost("dev")]
		[HttpPut("dev")]
		[HttpPatch("dev")]
		[HttpDelete("dev")]
		public IActionResult ErrorInDev()
		{
			return ProcessError(true);
		}

		protected IActionResult ProcessError(bool isDev)
		{
			var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
			var problemDetails = GetProblemDetails(feature, isDev);

			_logger.LogError(BuildErrorString(feature));
			return (problemDetails.Status == null) ? StatusCode(501, problemDetails) : StatusCode(problemDetails.Status.Value, problemDetails);
		}


		private ProblemDetails GetProblemDetails(IExceptionHandlerPathFeature feature, bool isDev)
		{
			var ex = feature?.Error ?? new Exception("Unknown Problem");

			//special cases
			if (ex is TrelloServiceException)
			{
				return new ProblemDetails
				{
					Status = (int)System.Net.HttpStatusCode.BadRequest,
					Instance = feature?.Path,
					Title = ex.Message,
					Detail = string.Join(" :: ", (ex as TrelloServiceException).Details)
				};

			}

			//else (all other exceptions)
			return new ProblemDetails
			{
				Status = (int)System.Net.HttpStatusCode.InternalServerError,
				Instance = feature?.Path,
				Title = isDev ? $"{ex.GetType().Name}: {ex.Message}" : "A server error occurred.",
				Detail = isDev ? ex.StackTrace : null,
			};
		}

		private string BuildErrorString(IExceptionHandlerPathFeature feature)
		{
			if (feature == null) { return "no error details available"; }
			var ex = feature?.Error ?? new Exception("Unknown Problem");

			return $"{feature.Path} - {ex.Message}";
		}
	}
}
