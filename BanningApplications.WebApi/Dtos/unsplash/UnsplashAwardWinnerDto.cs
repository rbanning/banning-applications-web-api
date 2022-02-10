namespace BanningApplications.WebApi.Dtos.unsplash
{
    public class UnsplashAwardWinnerDto: BaseAbstractExtendedDto
    {

		public string PhotoId { get; set; }

	    public int Year { get; set; }

		public string Category { get; set; }

		public bool Winner { get; set; }

		public virtual UnsplashPhotoDto Photo { get; set; }

    }
}
