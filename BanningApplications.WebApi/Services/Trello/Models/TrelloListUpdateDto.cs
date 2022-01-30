
using BanningApplications.WebApi.Services.Trello.Models.TrelloDtos;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloListUpdateDto
    {
		public string Name { get; set; }
		public bool? Closed { get; set; }

		public TrelloListUpdateDto()
		{ }

		public TrelloListUpdateDto(Dtos.PatchDto patch)
		{
			ApplyPatch(patch);
		}

		public TrelloListUpdateDto ApplyPatch(Dtos.PatchDto patch)
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

						//booleans
						case "/closed":
							if (bool.TryParse(patch.Value, out bool bresult))
							{
								Closed = bresult;
							}
							else
							{
								throw new TrelloServiceException("Unable to update board - invalid boolean patch value: " + (patch.Value ?? "null"));
							}
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
