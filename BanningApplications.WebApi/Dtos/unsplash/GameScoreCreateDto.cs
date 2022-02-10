using System.ComponentModel.DataAnnotations;
using BanningApplications.WebApi.Entities;

namespace BanningApplications.WebApi.Dtos.unsplash
{
    public class GameScoreCreateDto
    {
	    [Required, MaxLength(BaseMetaEntityUtil.EmailMaxLength)]
	    public string Email { get; set; }
	    [Required, MaxLength(BaseMetaEntityUtil.LongStringMaxLength)]
	    public string Game { get; set; }
	    [Required]
	    public decimal Points { get; set; }
	    [MaxLength(BaseMetaEntityUtil.SuperExtraLongStringMaxLength)]
	    public string Description { get; set; }

    }
}
