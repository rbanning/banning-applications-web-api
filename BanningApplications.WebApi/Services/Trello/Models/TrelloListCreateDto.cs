using BanningApplications.WebApi.Services.Trello.Models.TrelloDtos;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloListCreateDto
    {
	    public string Name { get; set; }
	    public string IdBoard { get; set; }
	    public string Pos { get; set; }	//position "top" or "bottom" (start or end)

	    public string Serialize()
	    {
		    return TrelloDtoSerializer.Serialize(this);
	    }

    }
}
