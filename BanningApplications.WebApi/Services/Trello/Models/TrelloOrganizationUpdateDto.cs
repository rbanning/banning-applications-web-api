
using BanningApplications.WebApi.Services.Trello.Models.TrelloDtos;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloOrganizationUpdateDto
    {
	    public string Name { get; set; }
	    public string DisplayName { get; set; }
	    public string Desc { get; set; }
	    public string Website { get; set; }

	    public TrelloOrganizationUpdateDto()
	    { }

	    public TrelloOrganizationUpdateDto(Dtos.PatchDto patch)
	    {
		    ApplyPatch(patch);
	    }

		public TrelloOrganizationUpdateDto ApplyPatch(Dtos.PatchDto patch)
		{

			switch (patch.Op)
			{
				case Dtos.PatchOperation.replace:
					switch (patch.Path.ToLower())
					{
						//simple string updates
						case "/name":
							Name = patch.Value;
							break;
						case "/displayname":
							DisplayName = patch.Value;
							break;
						case "/desc":
							Desc = patch.Value;
							break;
						case "/website":
							Website = patch.Value;
							break;

						default:
							throw new TrelloServiceException("Unable to update organization - invalid or unsupported patch path: " + (patch.Path ?? "null"));
					}


					break;
				case Dtos.PatchOperation.add:
				case Dtos.PatchOperation.remove:
				case Dtos.PatchOperation.delete:
					break;
				default:
					throw new TrelloServiceException("Unable to update organization - invalid or unsupported patch operation: " + (patch.Op.ToString()));
			}


			return this;
		}



		public string Serialize()
		{
			return TrelloDtoSerializer.Serialize(this);
		}

	}
}
