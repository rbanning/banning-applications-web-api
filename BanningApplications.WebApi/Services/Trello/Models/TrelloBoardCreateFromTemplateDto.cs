using System.ComponentModel.DataAnnotations;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloBoardCreateFromTemplateDto
    {
		[Required]
	    public string TemplateId { get; set; }
		[Required]
	    public string OrganizationId { get; set; }
		[Required]
	    public string Name { get; set; }
	    public string Description { get; set; }

    }
}
