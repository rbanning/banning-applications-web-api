using System;
using System.ComponentModel.DataAnnotations;

namespace BanningApplications.WebApi.Entities.unsplash
{
    public class UnsplashPhoto: BaseMetaExtendedEntity
    {
	    [Required, MaxLength(BaseMetaEntityUtil.IdStringMaxLength)]
	    public string UserName { get; set; }

	    public virtual UnsplashPhotographer Photographer { get; set; }

	    [Required] public int Width { get; set; }
	    [Required] public int Height { get; set; }

	    [Required] public DateTime Published { get; set; }
		
	    [Required, MaxLength(BaseMetaEntityUtil.IdStringMaxLength)]
	    public string BlurHash { get; set; }


	    [MaxLength(BaseMetaEntityUtil.LongStringMaxLength)]
	    public string Description { get; set; }

	    [MaxLength(BaseMetaEntityUtil.LongStringMaxLength)]
	    public string AltDescription { get; set; }

	    [MaxLength(BaseMetaEntityUtil.ShortStringMaxLength)]
	    public string Color { get; set; }

	    [MaxLength(BaseMetaEntityUtil.LongStringMaxLength)]
	    public string Location { get; set; }

	    public string TagsJson { get; set; }
	    public string TopicsJson { get; set; }
    }
}
