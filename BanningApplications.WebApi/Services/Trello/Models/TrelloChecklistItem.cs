using System;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloChecklistItem: TrelloAbstractBase
    {
		public string State { get; set; }
		public DateTime? Due { get; set; }
		public string IdMember { get; set; }

		public TrelloChecklistItem()
			: base()
		{ }
	}
}
