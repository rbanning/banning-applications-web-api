namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloMembership
    {
	    public enum MemberTypeEnum
	    {
			admin,
			normal,
			observer
	    }

	    public string Id { get; set; }
	    public string IdMember { get; set; }
	    public MemberTypeEnum MemberType { get; set; }
	    public bool Unconfirmed { get; set; }
	    public bool Deactivated { get; set; }
    }
}
