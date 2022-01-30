using System.Collections.Generic;

namespace BanningApplications.WebApi.Services.Trello.Models.TrelloDtos
{
    public class TrelloAddMemberToBoardDto
    {
	    public string Id { get; set; }
	    public List<TrelloMember> Members { get; set; }
        public List<TrelloMembership> Memberships { get; set; }
    }
}
