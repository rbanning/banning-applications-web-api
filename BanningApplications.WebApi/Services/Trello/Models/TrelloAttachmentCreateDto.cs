using BanningApplications.WebApi.Services.Trello.Models.TrelloDtos;
using Microsoft.AspNetCore.Http;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloAttachmentCreateDto
    {
	    public string Name { get; set; }
	    public string MimeType { get; set; }
	    public string Url { get; set; }
	    public bool SetCover { get; set; }

		public IFormFile File { get; set; }

	    public string Serialize()
	    {
		    return TrelloDtoSerializer.Serialize(this);
	    }

	}
}
