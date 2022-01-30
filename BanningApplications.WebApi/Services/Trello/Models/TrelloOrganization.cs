namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloOrganization: TrelloAbstract
    {
	    public string DisplayName { get; set; }
	    public string Website { get; set; }
	    public string LogoUrl { get; set; }
	    public string TeamType { get; set; }
	    public int? BillableMemberCount { get; set; }
	    public int? ActiveBillableMemberCount { get; set; }
    }
}
