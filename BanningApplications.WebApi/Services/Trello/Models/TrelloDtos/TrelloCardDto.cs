using System;

namespace BanningApplications.WebApi.Services.Trello.Models.TrelloDtos
{
    public class TrelloCardDto: TrelloAbstract
    {
	    public DateTime? Due { get; set; }

	    public DateTime? Start { get; set; }
		
	    public bool? DueComplete { get; set; }
	    public int? LabelCount { get; set; }
	    public DateTime DateLastActivity { get; set; }
		
	    public string IdBoard { get; set; }
	    public string IdList { get; set; }


	    //checklists
	    public int? ChecklistCount { get; set; }


	    //attachments
	    public int? AttachmentCount { get; set; }

			
	    //location
	    public string Address { get; set; }
	    public string LocationName { get; set; }
	    public TrelloCoords Coordinates { get; set; }

	    //members
	    public int? MemberCount { get; set; }

	    public TrelloCardDto()
	    {
		    
	    }

	    public TrelloCardDto(TrelloCard card)
			:base(card)
	    {
		    if (card != null)
		    {
			    Due = card.Due;
			    Start = card.Start;
			    DueComplete = card.DueComplete;
			    LabelCount = card.Labels?.Count;
			    DateLastActivity = card.DateLastActivity;
			    IdBoard = card.IdBoard;
			    IdList = card.IdList;
			    ChecklistCount = card.Checklists?.Count;
			    AttachmentCount = card.Attachments?.Count;
			    Address = card.Address;
			    LocationName = card.LocationName;
			    Coordinates = card.Coordinates;
			    MemberCount = card.Members?.Count;
		    }
		    
	    }
    }
}
