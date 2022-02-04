using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BanningApplications.WebApi.Entities;

namespace BanningApplications.WebApi.Dtos.unsplash
{
    public class UnsplashPhotoCreateDto
    {
        [MaxLength(BaseMetaEntityUtil.IdStringMaxLength)]
	    public string UserName { get; set; }

	    [Required] public int Width { get; set; }
	    [Required] public int Height { get; set; }

	    [Required] public DateTime Published { get; set; }
		
	    [MaxLength(BaseMetaEntityUtil.IdStringMaxLength)]
	    public string BlurHash { get; set; }


	    [MaxLength(BaseMetaEntityUtil.LongStringMaxLength)]
	    public string Description { get; set; }

	    [MaxLength(BaseMetaEntityUtil.LongStringMaxLength)]
	    public string AltDescription { get; set; }

	    [MaxLength(BaseMetaEntityUtil.ShortStringMaxLength)]
	    public string Color { get; set; }

	    public UnsplashLocationDto Location { get; set; }

	    public List<string> Tags { get; set; }
	    public List<string> Topics { get; set; }

		public UnsplashPhotoUrlsDto Urls { get; set; }
    }
}
