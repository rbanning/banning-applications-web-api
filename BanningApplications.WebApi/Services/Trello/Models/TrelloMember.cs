using System.Collections.Generic;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloMember
    {
	    public string Id { get; set; }
	    public string Username { get; set; }
	    public string FullName { get; set; }
	    public string Initials { get; set; }
	    public string AvatarUrl { get; set; }
	    public bool ActivityBlocked { get; set; }

		//Extended Properties (only available by getting member directly?)	
		public string Email { get; set; }
		public string Bio { get; set; }
		public List<string> IdBoards { get; set; }
		public List<string> IdOrganizations { get; set; }


		public TrelloMembership	Membership { get; set; }
    }
}
