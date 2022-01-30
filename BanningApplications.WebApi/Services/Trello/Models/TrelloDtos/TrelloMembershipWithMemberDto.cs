namespace BanningApplications.WebApi.Services.Trello.Models.TrelloDtos
{
	public class TrelloMembershipWithMemberDto
    {
		public string Id { get; set; }
		public string IdMember { get; set; }
		public TrelloMembership.MemberTypeEnum MemberType { get; set; }
		public bool Unconfirmed { get; set; }
		public bool Deactivated { get; set; }

		public virtual TrelloMemberWithoutMembershipDto Member { get; set; }
	}
}
