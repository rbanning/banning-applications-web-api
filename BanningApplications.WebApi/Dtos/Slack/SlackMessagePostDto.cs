using System.ComponentModel.DataAnnotations;

namespace BanningApplications.WebApi.Dtos.Slack
{
    public class SlackMessagePostDto
    {
        [Required, MaxLength(100)]  //todo: update Slack Channel length
	    public string Channel { get; set; }
        [Required, MaxLength(3000)] //todo: update Slack message length
	    public string Message { get; set; }
    }
}
