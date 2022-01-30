using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using BanningApplications.WebApi.Services.File;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BanningApplications.WebApi.Controllers
{
    [Route("c28699d2-196c-4c4f-a15d-ff3e631cd9d7/echo")]
    [ApiController]
    public class EchoController: ApiBaseController
    {
	    private readonly IWebHostEnvironment _host;
	    private readonly ILogger<EchoController> _logger;
	    private readonly List<string> _acceptedHeaders = new List<string>()
	    {
		    "connection", "accept", "accept-encoding","host", "user-agent", "x-auth","content-type"
		};

	    public EchoController(
		    IWebHostEnvironment host,
		    ILogger<EchoController> logger
		    )
	    {
		    _host = host;
		    _logger = logger;
	    }


		[HttpGet()]
	    public IActionResult Get()
	    {
			Log("GET");
		    return new OkObjectResult(new
		    {
				type = "get",
			    headers = HeaderValues(),
			    query = QueryParams(),
		    });
	    }

	    [HttpPost()]
	    public async Task<IActionResult> Post([FromQuery] bool save = false)
	    {
		    Log("POST");

		    var contentType = GetRequestContentType();
		    var content = await ReadPostContent();
		    bool? result = null;
		    if (save)
		    {
			    var now = DateTime.Now;
			    var filename = $"{now:yyyy-MMM-dd-HHmmss}-{content.Length}.txt";
			    var path = Path.Combine(_host.ContentRootPath, "logs", "posts", now.ToString("yyyy-MM"), filename);
			    var service = new FileService();
			    result = await service.SaveTextToFileAsync($"{contentType}\n****************\n{content}", path);
		    }

		    return new OkObjectResult(new
		    {
			    type = "post",
			    headers = HeaderValues(),
			    query = QueryParams(),
			    form = content,
				saved = result
		    });
	    }

	    [HttpPost("file")]
	    public async Task<IActionResult> PostTest2()
	    {
		    Log("POST");


		    string raw;
		    using (var stream = new StreamReader(Request.Body, Encoding.UTF8))
		    {
			    raw = await stream.ReadToEndAsync();
		    }

		    var action  = new OkObjectResult(new
		    {
			    type = "post",
			    headers = HeaderValues(),
			    query = QueryParams(),
				raw
		    });


		    return action;
	    }

	    [HttpPost("file-x")]
	    public async Task<IActionResult> PostTest([FromForm] PostForm form)
	    {
		    Log("POST");


		    var body = new
		    {
			    name = form.Name,
			    file = form.File != null
		    };

		    string raw;
		    using (var stream = new StreamReader(Request.Body, Encoding.UTF8))
		    {
			    raw = await stream.ReadToEndAsync();
		    }

		    var action  = new OkObjectResult(new
		    {
			    type = "post",
			    headers = HeaderValues(),
			    query = QueryParams(),
				body,
				raw,
				file = new {
					name = form.File?.Name,
					filename = form.File?.FileName,
					length = form.File?.Length,
					type = form.File?.ContentType
				}
		    });

		    if (string.Equals(form.Name, "save", StringComparison.CurrentCultureIgnoreCase))
		    {
			    var service = new FileService();

		    }

		    return action;
	    }


	    private void Log(string method)
	    {
		    try
		    {
			    var userAgent = Request.Headers?.FirstOrDefault(h =>
				    string.Equals(h.Key, "USER-AGENT", StringComparison.CurrentCultureIgnoreCase));
			    var type = GetRequestContentType();
			    _logger.LogInformation($"{method.ToUpper()} Echo ({type}) from {userAgent?.Value}", userAgent);
		    }
			catch (Exception)
			{
				//todo: ignore
			}
	    }

	    private Dictionary<string, string> HeaderValues()
	    {
		    return this.Request.Headers?
			    .Where(h => _acceptedHeaders.Contains(h.Key.ToLower()))
			    .ToDictionary(h => h.Key, h => h.Value.ToString());

			
	    }

	    private Dictionary<string, string> QueryParams()
	    {
		    return Request.Query?.ToDictionary(q => q.Key, q => q.Value.ToString());
	    }


	    public class PostForm
	    {
		    public string Name { get; set; }
		    public IFormFile File { get; set; }
	    }


	    private string GetRequestContentType()
	    {
		    var contentType = Request.Headers.FirstOrDefault(m => string.Equals(m.Key, "content-type", StringComparison.CurrentCultureIgnoreCase)).Value;
		    return contentType.FirstOrDefault();
	    }

	    private async Task<string> ReadPostContent()
	    {
		    var type = GetRequestContentType() ?? "unknown";
		    type = type.ToLower();
		    try
		    {
			    switch (type)
			    {
				    case "application/json":
					    return await ReadBody();

					default:
						return type.Contains("form-data")
							? ReadForm()
							: $"Unsupported Content Type: {type}";
			    }
		    }
		    catch (Exception e)
		    {
			    return $"Error reading post content of type - {type} - {e.Message}";
		    }
	    }

	    private async Task<string> ReadBody()
	    {
		    using var reader = new StreamReader(Request.Body, Encoding.UTF8);
		    return await reader.ReadToEndAsync();
	    }

	    private string ReadForm()
	    {
		    var result = "";
		    foreach (var pair in Request.Form)
		    {
			    result += $"{pair.Key}={pair.Value.FirstOrDefault()}\n";
		    }

		    return result;
	    }
    }
}
