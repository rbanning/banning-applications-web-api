namespace BanningApplications.WebApi.Services.Trello.Models.TrelloDtos
{
    public class TrelloMemberWithoutMembershipDto
    {
		public string Id { get; set; }
		public string Username { get; set; }
		public string FullName { get; set; }
		public string Initials { get; set; }
		public string AvatarUrl { get; set; }
		public bool ActivityBlocked { get; set; }

	}
}
