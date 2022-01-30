using System.ComponentModel.DataAnnotations;

namespace BanningApplications.WebApi.Services.Trello.Models.TrelloDtos
{
    public class TrelloMoveCardBasedOnDateDto
    {
	    public enum Position
	    {
            top,
            bottom
	    }

        [Required]
        public string TargetListId { get; set; }

        public string Min { get; set; }

        public string Max { get; set; }

        public bool UseStartIfExists { get; set; }

        public Position Pos { get; set; }

        public TrelloMoveCardBasedOnDateDto()
        {
	        Pos = Position.bottom;
        }
    }
}
