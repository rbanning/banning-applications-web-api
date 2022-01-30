using System;
using System.Collections.Generic;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloBoard: TrelloAbstract
    {
		public string IdOrganization { get; set; }
		public bool? Pinned { get; set; }
		public bool Starred { get; set; }
		public TrelloPrefs Prefs { get; set; }
		public TrelloLabelNames LabelNames { get; set; }
		public List<TrelloMembership> Memberships { get; set; }
		public DateTime? DateLastActivity { get; set; }
		public List<TrelloChecklist> Checklists { get; set; }
		public List<TrelloCustomField> CustomFields { get; set; }

		public List<TrelloList> Lists { get; set; }
    }
}
