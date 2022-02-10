using System.ComponentModel.DataAnnotations;
using BanningApplications.WebApi.Entities;

namespace BanningApplications.WebApi.Dtos.unsplash
{
    public class UnsplashAwardWinnerCreateDto
    {
	    [Required, MaxLength(BaseMetaEntityUtil.IdGuidMaxLength)]
		public string PhotoId { get; set; }

		[Required]
	    public int Year { get; set; }

		[Required, MaxLength(BaseMetaEntityUtil.IdStringMaxLength)]
		public string Category { get; set; }

		[Required]
		public bool Winner { get; set; }

    }
}
