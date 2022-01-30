
using System.Collections.Generic;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloCardLinkedCreateDto: TrelloCardCreateDto
    {
	    public string TrelloCardId { get; set; }

        public TrelloStickerBase Sticker { get; set; }

        public bool AddLinkOnOriginalCard { get; set; }

        public IList<TrelloCardCustomFieldLookupDto> CustomFields { get; set; }

        public TrelloCardLinkedCreateDto()
        {
	        AddLinkOnOriginalCard = true;   //default
	        CustomFields = new List<TrelloCardCustomFieldLookupDto>();
        }
    }
}
