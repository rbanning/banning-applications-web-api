namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloCardCustomFieldLookupDto
    {
	    public string Name { get; set; }
	    public string Type { get; set; }    //"checkbox", "list", "date", "text", "number"
		public string Value { get; set; }
    }
}
