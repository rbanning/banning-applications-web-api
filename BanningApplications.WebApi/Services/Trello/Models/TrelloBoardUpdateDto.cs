using System.Text.Json.Serialization;
using BanningApplications.WebApi.Services.Trello.Models.TrelloDtos;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloBoardUpdateDto
    {
		public string Name { get; set; }
		public string Desc { get; set; }
		public bool? Closed { get; set; }

		[JsonPropertyName("prefs/background")]
		public string PrefsBackground { get; set; }

		[JsonPropertyName("labelNames/green")]
		public string LabelNamesGreen { get; set; }
		[JsonPropertyName("labelNames/yellow")]
		public string LabelNamesYellow { get; set; }
		[JsonPropertyName("labelNames/orange")]
		public string LabelNamesOrange { get; set; }
		[JsonPropertyName("labelNames/red")]
		public string LabelNamesRed { get; set; }
		[JsonPropertyName("labelNames/purple")]
		public string LabelNamesPurple { get; set; }
		[JsonPropertyName("labelNames/blue")]
		public string LabelNamesBlue { get; set; }


		public TrelloBoardUpdateDto()
		{ }

		public TrelloBoardUpdateDto(Dtos.PatchDto patch)
		{
			this.ApplyPatch(patch);
		}

		public TrelloBoardUpdateDto ApplyPatch(Dtos.PatchDto patch)
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
						case "/desc":
							Desc = patch.Value;
							break;

						case "/prefs/background":
							PrefsBackground = patch.Value;
							break;

						case "/label/green":
						case "/labels/green":
						case "labelnames/green":
							LabelNamesGreen = patch.Value;
							break;

						case "/label/yellow":
						case "/labels/yellow":
						case "labelnames/yellow":
							LabelNamesYellow = patch.Value;
							break;

						case "/label/orange":
						case "/labels/orange":
						case "labelnames/orange":
							LabelNamesOrange = patch.Value;
							break;

						case "/label/red":
						case "/labels/red":
						case "labelnames/red":
							LabelNamesRed = patch.Value;
							break;

						case "/label/purple":
						case "/labels/purple":
						case "labelnames/purple":
							LabelNamesPurple = patch.Value;
							break;

						case "/label/blue":
						case "/labels/blue":
						case "labelnames/blue":
							LabelNamesBlue = patch.Value;
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
							throw new TrelloServiceException("Unable to update board - invalid or unsupported patch path: " + (patch.Path ?? "null"));
					}


					break;
				case Dtos.PatchOperation.add:
				case Dtos.PatchOperation.remove:
				case Dtos.PatchOperation.delete:
					break;
				default:
					throw new TrelloServiceException("Unable to update board - invalid or unsupported patch operation: " + (patch.Op.ToString()));
			}


			return this;
		}



		public string Serialize()
		{
			return TrelloDtoSerializer.Serialize(this);
		}
	}
}
