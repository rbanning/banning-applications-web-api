using BanningApplications.WebApi.Helpers;

namespace BanningApplications.WebApi.Dtos.unsplash
{
    public class UnsplashPhotoUrlsDto
    {
	    public string Raw { get; set; }
	    public string Full { get; set; }
	    public string Regular { get; set; }
	    public string Small { get; set; }
	    // ReSharper disable once InconsistentNaming
	    public string Small_s3 { get; set; }
	    public string Thumb { get; set; }


	    public static UnsplashPhotoUrlsDto DeserializeFrom(string json)
	    {
		    return json.Deserialize<UnsplashPhotoUrlsDto>();
	    }
    }
}
