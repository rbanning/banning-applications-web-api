using System;
using System.Collections.Generic;
using BanningApplications.WebApi.Helpers;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloCard: TrelloAbstract
    {
		public DateTime? Due { get; set; }

		public int? DueOffset => Due.DateOffset();

		//else
		public DateTime? Start { get; set; }
		public int? StartOffset => Start.DateOffset();
		
		public bool? DueComplete { get; set; }
		public List<TrelloLabel> Labels { get; set; }
		public DateTime DateLastActivity { get; set; }
		
		public string IdBoard { get; set; }
		public string IdList { get; set; }


		//checklists
		public virtual List<TrelloChecklist> Checklists { get; set; }


		//attachments
		public virtual List<TrelloAttachment> Attachments { get; set; }

			
		//location
		public string Address { get; set; }
		public string LocationName { get; set; }
		public TrelloCoords Coordinates { get; set; }

		//members
		public virtual List<TrelloMember> Members { get; set; }

		public TrelloCard()
		{
			Labels = new List<TrelloLabel>();
		}
	}
}
