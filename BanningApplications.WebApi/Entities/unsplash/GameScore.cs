using System.ComponentModel.DataAnnotations;

namespace BanningApplications.WebApi.Entities.unsplash
{
    public class GameScore: BaseMetaExtendedEntity
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
