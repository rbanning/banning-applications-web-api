using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace BanningApplications.WebApi.Controllers
{
    [Route("/logs")]
    [Authorize()]
    [ApiController]
    public class LogsController: ApiBaseController
    {
	    private readonly IWebHostEnvironment _hostingEnvironment;
	    private readonly ILogger _logger;

	    private string CurrentDirectory => _hostingEnvironment.ContentRootPath;

	    public LogsController(
		    IWebHostEnvironment hostingEnvironment,
		    ILogger<LogsController> logger)
	    {
		    _hostingEnvironment = hostingEnvironment;
		    _logger = logger;
	    }

	    [HttpGet("posts/{id}")]
	    public IActionResult GetPosts([FromRoute] string id)
	    {
		    var path = Path.Combine(CurrentDirectory, "logs", "posts", id);
		    if (!Directory.Exists((path)))
		    {
			    return NotFound();

		    }
		    var files = Directory.GetFiles(path);
		    if (files == null)
		    {
			    return NotFound();
		    }

		    return new OkObjectResult(files.Select(Path.GetFileName).ToArray());
	    }

	    [HttpGet("posts/{id}/{filename}")]
	    public async Task<IActionResult> GetPosts([FromRoute] string id, [FromRoute] string filename)
	    {
		    var path = Path.Combine(CurrentDirectory, "logs", "posts", id, filename);
		    if (!System.IO.File.Exists((path)))
		    {
			    return NotFound();

		    }

		    var content = await System.IO.File.ReadAllTextAsync(path);

		    return Content(content, MediaTypeHeaderValue.Parse("text/plain"));
	    }

	    [HttpGet("{id}")]
	    public IActionResult Get([FromRoute] string id)
	    {
		    var path = Path.Combine(CurrentDirectory, "logs", id);
		    if (!Directory.Exists((path)))
		    {
			    return NotFound();

		    }
		    var files = Directory.GetFiles(path);
		    if (files == null)
		    {
			    return NotFound();
		    }

		    return new OkObjectResult(files.Select(Path.GetFileName).ToArray());
	    }

	    [HttpGet("{year}/{month}/{day}")]
	    public async Task<IActionResult> Get(
		    [FromRoute] int year, 
		    [FromRoute] byte month, 
		    [FromRoute] byte day,
			[FromQuery] string type = "hallpass",
		    [FromQuery] string format = "json")
	    {
		    var usr = GetAppUser();
		    if (usr == null) { return new UnauthorizedResult(); }

		    type ??= "hallpass";
		    format ??= "json";


			var stYear = year.ToString();
		    var stMonth = month.ToString().PadLeft(2, '0');
		    var stDay = day.ToString().PadLeft(2, '0');

		    var filename = $"nlog-{type}-{stYear}-{stMonth}-{stDay}.log";
		    var path = Path.Combine(CurrentDirectory, "logs", $"{stYear}-{stMonth}", filename);
		    var exists = System.IO.File.Exists(path);
			Log(usr, filename, exists);

			if (!exists)
			{
				return NotFound(_hostingEnvironment.IsDevelopment() ? path : null);
			}

			//else
			var text = await System.IO.File.ReadAllTextAsync(path);
			switch (format.ToLower())
			{
				case "text":
					return new OkObjectResult(text);
				case "json":
					return new OkObjectResult(text.Split("\n").Select(row => row.Split("|")));
				default:
					return BadRequest("Invalid format requested");

			}
	    }


	    // ReSharper disable once UnusedParameter.Local
	    private void Log(Identity.AppUser usr, string filename, bool exists)
	    {
		    var status = exists ? "Success" : "Failed";
			_logger.LogInformation($"Request for Log: {filename} - {status}");
	    }
	}
}
