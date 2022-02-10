namespace BanningApplications.WebApi.Dtos.unsplash
{
    public class GameScoreDto: BaseAbstractExtendedDto
    {
	    public string Email { get; set; }
	    public string Game { get; set; }
	    public decimal Points { get; set; }
	    public string Description { get; set; }
    }
}
