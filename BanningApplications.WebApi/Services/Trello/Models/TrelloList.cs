using System.Collections.Generic;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloList: TrelloAbstractBase
    {
		public string IdBoard { get; set; }
		public bool Closed { get; set; }

		public List<TrelloCard> Cards { get; set; }

		public TrelloList()
			:base()
		{
			//this.Cards = new List<TrelloCard>();
		}
    }
}
