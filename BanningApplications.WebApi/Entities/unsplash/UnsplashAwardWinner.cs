using System;
using System.ComponentModel.DataAnnotations;

namespace BanningApplications.WebApi.Entities.unsplash
{
    public class UnsplashAwardWinner: BaseMetaExtendedEntity
    {
		[Required, MaxLength(BaseMetaEntityUtil.IdGuidMaxLength)]
		public string PhotoId { get; set; }

		[Required]
	    public int Year { get; set; }

		[Required, MaxLength(BaseMetaEntityUtil.IdStringMaxLength)]
		public string Category { get; set; }

		public virtual UnsplashPhoto Photo { get; set; }

    }
}
