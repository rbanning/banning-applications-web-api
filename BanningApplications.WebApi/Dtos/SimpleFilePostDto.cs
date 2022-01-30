using Microsoft.AspNetCore.Http;

namespace BanningApplications.WebApi.Dtos
{
	public class SimpleFilePostDto
    {
	    public IFormFile File { get; set; }
	    public string Name { get; set; }
    }
}
