using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using BanningApplications.WebApi.Dtos;
using BanningApplications.WebApi.Helpers;
using BanningApplications.WebApi.Services.Trello.Models.TrelloDtos;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloCardUpdateDto
    {
		public string Name { get; set; }
		public string Desc { get; set; }
		public bool? Closed { get; set; }
		public string IdList { get; set; }
		public string IdBoard { get; set; }
		public string Pos { get; set; }
		public DateTime? Due { get; set; }
		public DateTime? Start { get; set; }
		public bool? DueComplete { get; set; }
		public string Address { get; set; }
		public string LocationName { get; set; }
		public string Coordinates { get; set; }		//latitude,longitude
		public TrelloCardCover Cover { get; set; }

		[JsonIgnore]
		public List<string> ExtraJson { get; set; }


		public TrelloCardUpdateDto()
		{
			ExtraJson = new List<string>();
		}

		public TrelloCardUpdateDto(PatchDto patch)
			:this()
		{
			ApplyPatch(patch);
		}

		public void ClearCover()
		{
			ExtraJson.Add(@"""cover"": """" ");
		}

		public TrelloCardUpdateDto ApplyPatch(PatchDto patch)
		{

			switch (patch.Op)
			{
				case PatchOperation.replace:
					ApplyPatchForReplaceOperations(patch);
					break;
				case PatchOperation.add:
					ApplyPatchForAddOperations(patch);
					break;
				case PatchOperation.remove:
				case PatchOperation.delete:
					ApplyPatchForRemoveOperations(patch);
					break;
				default:
					throw new TrelloServiceException("Unable to update card - invalid or unsupported patch operation: " + (patch.Op.ToString()));
			}


			return this;
		}

		private void ApplyPatchForReplaceOperations(PatchDto patch)
		{
			if (patch?.Op != PatchOperation.replace)
			{
				throw new ArgumentException("Expected ADD Patch Operation");
			}

			//else
			switch (patch.Path.ToLower())
			{
				//simple string updates
				case "/name":
					Name = patch.Value;
					break;
				case "/desc":
					Desc = patch.Value;
					break;
				case "/list":
				case "/listid":
				case "/idlist":
					IdList = patch.Value;
					break;
				case "/board":
				case "/boardid":
				case "/idboard":
					IdBoard = patch.Value;
					break;
				case "/pos":
				case "/position":
					Pos = patch.Value;
					break;
				case "/address":
					Address = patch.Value;
					break;
				case "/location":
				case "/locationname":
					LocationName = patch.Value;
					break;
				case "/coordinates":
					Coordinates = patch.Value;      //should be latitude,longitude
					break;
				case "/cover":
					if (string.IsNullOrEmpty(patch.Value))
					{
						ClearCover();
					}
					else
					{
						Cover = new TrelloCardCover(patch.Value);
					}
					break;

				//booleans
				case "/closed":
				case "/duecomplete":
					if (bool.TryParse(patch.Value, out bool bresult))
					{
						if (patch.Path.ToLower() == "/closed")
						{
							Closed = bresult;
						}
						else
						{
							DueComplete = bresult;
						}
					}
					else
					{
						throw new TrelloServiceException("Unable to update card - invalid boolean patch value: " + (patch.Value ?? "null"));
					}
					break;

				//date
				case "/due":
				case "/start":

					if (string.IsNullOrEmpty(patch.Value))
					{
						SetDate(patch.Path, null);
					}
					else if (DateTime.TryParse(patch.Value, out DateTime dresult))
					{
						SetDate(patch.Path, dresult);
					}
					else
					{
						var queryResult = patch.Value.QueryDateTime();
						if (queryResult.HasValue)
						{
							SetDate(patch.Path, queryResult);
						}
						else
						{
							throw new TrelloServiceException("Unable to update card - invalid datetime patch value: " + (patch.Value ?? "null"));
						}
					}
					break;


				default:
					throw new TrelloServiceException("Unable to update card - invalid or unsupported patch REPLACE path: " + (patch.Path ?? "null"));
			}
		}
		
		private void ApplyPatchForAddOperations(PatchDto patch)
		{
			if (patch?.Op != PatchOperation.add)
			{
				throw new ArgumentException("Expected ADD Patch Operation");
			}

			//else
			switch (patch.Path.ToLower())
			{
				//todo: is there anything we want to add to a Trello card via PATCH?
				case "/cover":
					Cover = new TrelloCardCover(patch.Value);
					break;

				default:
					throw new TrelloServiceException("Unable to update card - invalid or unsupported patch ADD path: " + (patch.Path ?? "null"));
			}
		}

		private void ApplyPatchForRemoveOperations(PatchDto patch)
		{
			if (patch?.Op != PatchOperation.remove && patch?.Op != PatchOperation.delete)
			{
				throw new ArgumentException("Expected ADD Patch Operation");
			}

			//else
			switch (patch.Path.ToLower())
			{
				//todo: is there anything we want to remove to a Trello card via PATCH?
				case "/cover":
					if (string.IsNullOrEmpty(patch.Value))
					{
						ClearCover();
					}
					break;

				default:
					throw new TrelloServiceException("Unable to update card - invalid or unsupported patch REMOVE path: " + 
					                                 (patch.Path ?? "null"));
			}
		}


		private void SetDate(string path, DateTime? date)
		{
			if (string.Equals(path, "/start", StringComparison.CurrentCultureIgnoreCase))
			{
				Start = date;
			} 
			else if (string.Equals(path, "/due", StringComparison.CurrentCultureIgnoreCase))
			{
				Due = date;
			}
			else
			{
				throw new ArgumentException("Could not set card date: " + path);
			}
		}
		public string Serialize()
		{
			var result = TrelloDtoSerializer.Serialize(this);
			if (ExtraJson?.Count > 0)
			{
				if (result.StartsWith("{") && result.EndsWith("}"))
				{
					result = result.Length == 2 ? "" : result.Substring(1, result.Length - 2).Trim();
				}
				result = "{"
				         + string.Join(",", ExtraJson)
				         + (result.Length > 0 ? "," : "")
				         + "}";
			}
			return result;
		}
	}
}
