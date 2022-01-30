using System.Collections.Generic;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloProMeetingCard
    {
		public string Id { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string Icon { get; set; }
		public byte Order { get; set; }
		public string ListType { get; set; }

		public TrelloCard Card { get; set; }

		//members
		public List<TrelloMember> Members { get; set; }

	}
}
