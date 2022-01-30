using System.Collections.Generic;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloChecklist: TrelloAbstractBase
    {

		public string IdCard { get; set; }

		public List<TrelloChecklistItem> CheckItems { get; set; }

		public TrelloChecklist()
			:base()
		{
			this.CheckItems = new List<TrelloChecklistItem>();
		}
    }
}
