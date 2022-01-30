using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BanningApplications.WebApi.Helpers;
using BanningApplications.WebApi.Services.File;
using BanningApplications.WebApi.Services.Trello.Models;
using BanningApplications.WebApi.Services.Trello.Models.TrelloDtos;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;

// ReSharper disable PossibleMultipleEnumeration

// ReSharper disable InconsistentNaming

namespace BanningApplications.WebApi.Services.Trello
{
	public enum ProMeetingListType
	{
		pending,
		active,
		done,
		unknown
	}

	public interface ITrelloService
	{
		void Configure(TrelloConfig config);
		bool IsConfigured { get; }
		TrelloConfig Config { get; }

		#region >> GENERAL METHODS <<

		Task<TrelloChecklist> GetChecklistAsync(string id);

		Task<Stream> DownloadAttachment(string url);

		//HELPERS

		string BuildUrlForBoard(string boardId);
		string BuildUrlForCard(string cardId);

		Task<List<TrelloLabel>> SearchBoardLabels(string boardId, List<string> lookupList);

		#endregion

		#region >> CARDS <<

		Task<List<TrelloCard>> GetCardsOnBoardAsync(string id, bool inclClosed);
		Task<List<TrelloCard>> GetCardsOnListAsync(string id);

		Task<bool> CardExistsAsync(string id);

		Task<TrelloCard> GetCardAsync(string id);
		Task<TrelloCard> GetCardAsync(string id, bool inclChecklists, bool inclAttachments, bool inclMembers);
		Task<TrelloCard> GetCardByNameAsync(string boardId, string name, bool inclChecklists, bool inclAttachments, bool inclMembers);

		Task<IEnumerable<TrelloMember>> GetCardMembersAsync(string id);

		Task<TrelloCard> PatchCardAsync(string id, TrelloCardUpdateDto dto);
		Task<TrelloCard> PatchCardAsync(string id, Dtos.PatchDto patch);
		Task<TrelloCard> PatchCardAsync(string id, List<Dtos.PatchDto> patches);

		Task<TrelloCard> SetCardCover(string id, TrelloCardCover cover);
		Task<TrelloCard> RemoveCardCover(string id);

		Task<TrelloCard> CreateCardAsync(TrelloCardCreateDto model);
		Task<bool> DeleteCardAsync(string id);

		Task<List<TrelloLabel>> AddCardLabelAsync(string id, string labelId);
		Task<List<TrelloLabel>> RemoveCardLabelAsync(string id, string labelId);

		//stickers
		Task<List<TrelloSticker>> GetCardStickersAsync(string id);
		Task<TrelloSticker> AddCardStickerAsync(string id, TrelloStickerBase sticker);
		Task<List<TrelloSticker>> RemoveCardStickerAsync(string id, string stickerId);

		//attachments
		Task<List<TrelloAttachment>> GetCardAttachmentsAsync(string id);
		Task<TrelloAttachment> AddCardUrlAttachmentAsync(string id, string url, string name, bool setCover = false);
		Task<TrelloAttachment> AddCardAttachmentsAsync(string id, TrelloAttachmentCreateDto model);
		Task<TrelloAttachment> AddCardAttachmentsAsync(string id, string path, string contentType, string name = null, bool setCover = false);

		Task<bool> RemoveCardAttachmentsAsync(string id, string attachmentId);

		//actions 
		Task<List<TrelloAction>> GetCardActions(string id, string actionType);
		Task<TrelloAction> AddCardComment(string id, string text);

		#endregion

		#region >> LISTS <<

		Task<List<TrelloList>> GetListsOnBoardAsync(string id, bool inclClosed, bool inclCards);

		Task<TrelloList> GetListAsync(string id);

		Task<TrelloList> PatchListAsync(string id, Dtos.PatchDto patch);
		Task<TrelloList> PatchListAsync(string id, List<Dtos.PatchDto> patches);


		Task<TrelloList> CreateListAsync(TrelloListCreateDto model);

		#endregion


		#region >> BOARDS <<

		Task<bool> BoardExistsAsync(string id);

		Task<List<TrelloBoard>> GetMyBoardsAsync(bool detailed);

		Task<TrelloBoard> GetBoardAsync(string id, bool inclChecklists, bool inclCustomFields);
		Task<TrelloBoard> GetBoardByNameAsync(string name, bool inclChecklists, bool inclCustomFields);

		Task<List<TrelloLabel>> GetBoardLabelsAsync(string id);

		Task<List<TrelloChecklist>> GetBoardChecklistsAsync(string id);

		Task<List<TrelloCard>> GetBoardCardsAsync(string id);

		Task<TrelloBoard> CreateBoardFromAsync(string boardSourceId, string orgId, string name, string description, bool copyCards = false);

		Task<TrelloBoard> PatchBoardAsync(string id, Dtos.PatchDto patch);
		Task<TrelloBoard> PatchBoardAsync(string id, List<Dtos.PatchDto> patches);

		
		#endregion

		#region >> CUSTOM FIELDS <<

		Task<TrelloCustomField> GetCustomFieldAsync(string id);

		Task<IEnumerable<TrelloCustomField>> GetCustomFieldsOnBoardAsync(string id);
		Task<IEnumerable<TrelloCardCustomFieldValue>> GetCustomFieldsOnCardAsync(string id, bool inclFieldDef);

		Task<TrelloCardCustomFieldValue> SetCustomFieldOnCardAsync(string id, string customFieldDefId, string type,
			string value);


		Task<TrelloCardCustomFieldValue> SetCustomFieldOnCardAsync(string id, IEnumerable<TrelloCustomField> customFieldDefs, TrelloCardCustomFieldLookupDto dto);

		#endregion

		#region >> ORGANIZATION <<

		Task<IEnumerable<TrelloOrganization>> GetOrganizationsAsync();
		Task<TrelloOrganization> GetOrganizationAsync(string id);

		Task<IEnumerable<TrelloBoard>> GetOrganizationBoards(string id);


		Task<TrelloOrganization> PatchOrganizationAsync(string id, Dtos.PatchDto patch);
		Task<TrelloOrganization> PatchOrganizationAsync(string id, List<Dtos.PatchDto> patches);

		#endregion

		#region >> Members Methods <<

		Task<TrelloMember> GetMemberAsync(string id);

		Task<List<TrelloMember>> GetMembersInOrganizationAsync(string id);

		Task<TrelloMember> FindMemberByUsernameAsync(string username, string orgId = null);

		//Cards
		Task<List<TrelloMember>> GetMembersOnCardAsync(string id);

		Task<List<TrelloMember>> AddMemberToCardAsync(string cardId, string memberId);

		Task<List<TrelloMember>> RemoveMemberFromCardAsync(string cardId, string memberId);


		Task<List<TrelloCard>> GetAllCardsMemberIsOn(string memberId, bool inclClosed);

		Task<List<TrelloMember>> EnsureMemberIsOnCardAsync(string cardId, string memberId);


		//Boards

		Task<List<TrelloMember>> GetMembersOnBoardAsync(string id);

		Task<List<TrelloMember>> AddMemberToBoardAsync(string boardId, string memberId,
			TrelloMembership.MemberTypeEnum memberType);

		Task<TrelloMember> UpdateMemberOnBoardAsync(string boardId, string memberId,
			TrelloMembership.MemberTypeEnum memberType);

		Task<List<TrelloMember>> RemoveMemberFromBoardAsync(string boardId, string memberId);

		#endregion

	}
	public class TrelloService: ITrelloService
	{
		private const string HTTP_NAME = "trello";
		private const string ENTITY_FIELDS_MIN = "id,name,closed";
		private const string BOARD_FIELDS = "id,name,desc,idOrganization,pinned,starred,prefs,labelNames,memberships,dateLastActivity,url,closed";
		private const string BOARD_FIELDS_LITE = "id,name,desc,idOrganization,starred,memberships,dateLastActivity,url,closed";
		private const string BOARD_LABEL_FIELDS = "id,name,color";
		private const string LIST_FIELDS = "all";
		private const string CARD_FIELDS = "id,name,desc,due,start,dueComplete,labels,url,address,locationName,coordinates,dateLastActivity,closed,idBoard,idList";
		private const string MEMBER_FIELDS = "id,username,fullName,initials,avatarUrl,activityBlocked";
		private const string MEMBER_FIELDS_EXTENDED = "id,username,fullName,initials,avatarUrl,activityBlocked,email, bio,idBoards,idOrganizations";
		private const string ATTACHMENT_FIELDS = "id,name,date,isUploaded,mimeType,url,fileName";

		//the following are the only mimeTypes accepted for uploaded attachments
		private readonly Dictionary<string, string> attachmentMimeTypes = new Dictionary<string, string>()
		{
			{"image/png", ".png"},
			{"image/jpeg", ".jpg"},
			{"image/jpg", ".jpg"},
			{"image/gif", ".gif"},
			{"application/pdf", ".pdf"}
		};

		private TrelloConfig _config;
		private readonly IHttpClientFactory _clientFactory;

		public TrelloService(
			IHttpClientFactory clientFactory)
		{
			_clientFactory = clientFactory;
		}

		public void Configure(TrelloConfig config)
		{
			_config = config;
		}

		public bool IsConfigured => _config != null && _config.IsValid;

		public TrelloConfig Config => _config;



		#region >> GENERAL METHODS <<



		public async Task<TrelloChecklist> GetChecklistAsync(string id)
		{
			var url = BuildUrl($"checklists/{id}");
			return await PerformGetAsync<TrelloChecklist>(url);
		}


		public async Task<Stream> DownloadAttachment(string url)
		{
			if (string.IsNullOrEmpty(url))
			{
				throw new ArgumentNullException(nameof(url));
			}

			if (!url.ToLower().StartsWith("https://trello.com"))
			{
				throw new ArgumentException("Not a Trello Url");
			}

			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Authorization = AuthHeader();
			var client = _clientFactory.CreateClient(HTTP_NAME);
			var response = await client.SendAsync(request);


			if (response.IsSuccessStatusCode)
			{
				return await response.Content.ReadAsStreamAsync();
			}
			if (response.StatusCode != HttpStatusCode.NotFound)
			{
				await ThrowTrelloServiceError(response);
			}

			//else (not found)
			return null;
		}



		public string BuildUrlForBoard(string boardId)
		{
			return BuildUrlFor("b", boardId);
		}

		public string BuildUrlForCard(string cardId)
		{
			return BuildUrlFor("c", cardId);
		}

		protected string BuildUrlFor(string what, string id)
		{
			return $"https://trello.com/{what}/{id}";
		}


		// Gets a boards labels and only returns those that match the lookup information (in lookupList)
		// lookupList can contain...
		//		list id (full text)
		//		partial name (prefix with *)
		//		color (prefix with -)
		public async Task<List<TrelloLabel>> SearchBoardLabels(string boardId, List<string> lookupList)
		{
			var labels = await GetBoardLabelsAsync(boardId);

			return lookupList
				.Select(lookup =>
				{
					if (string.IsNullOrEmpty(lookup))
					{
						return null;
					}

					if (lookup.StartsWith("*"))
					{
						//look for the name (CONTAINS)
						return labels.FirstOrDefault(l =>
							l.Name.Contains(lookup.Substring(1), StringComparison.CurrentCultureIgnoreCase));
					}

					if (lookup.StartsWith("+"))
					{
						//look for the color (EQUALS)
						return labels.FirstOrDefault(l =>
							l.Color.Equals(lookup.Substring(1), StringComparison.CurrentCultureIgnoreCase));
					}


					//else look for the id (EQUALS)
					return labels.FirstOrDefault(l =>
						l.Id.Equals(lookup, StringComparison.CurrentCultureIgnoreCase));
				})
				.Where(result => result != null)
				.ToList();

		}


		#endregion


		#region >> CARDS <<

		public async Task<bool> CardExistsAsync(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return false;
			}

			var url = BuildUrl($"cards/{id}", new Dictionary<string, string>() { 
				{ "fields", "id" },
				{ "checklists", "none" },
				{ "attachments", "false" },
				{ "members", "false" }
			});

			TrelloCard result;
			try
			{
				result = await PerformGetAsync<TrelloCard>(url);
			}
			catch (TrelloServiceException e)
			{
				if (e.Details.Any(m => string.Equals(m, "invalid id", StringComparison.CurrentCultureIgnoreCase)))
				{
					result = null;
				}
				else
				{
					throw;
				}
			}
			return result != null;
		}


		public async Task<List<TrelloCard>> GetCardsOnBoardAsync(string id, bool inclClosed)
		{
			var filter = inclClosed ? "all" : "open";
			var url = BuildUrl($"boards/{id}/cards/{filter}");

			var cards = await PerformGetAsync<List<TrelloCard>>(url);

			return NormalizeResult(cards);

		}

		public async Task<List<TrelloCard>> GetCardsOnListAsync(string id)
		{
			var url = BuildUrl($"lists/{id}/cards");

			var cards = await PerformGetAsync<List<TrelloCard>>(url);

			return NormalizeResult(cards);

		}


		public async Task<TrelloCard> GetCardAsync(string id)
		{
			return await GetCardAsync(id, false, false, false);
		}
		public async Task<TrelloCard> GetCardAsync(string id, bool inclChecklists, bool inclAttachments, bool inclMembers)
		{
			var url = BuildUrl($"cards/{id}", new Dictionary<string, string>() { { "fields", CARD_FIELDS },
				{ "checklists", inclChecklists ? "all" : "none" },
				{ "attachments", inclAttachments ? "true" : "false" },
				{ "members", inclMembers ? "true" : "false" }
			});

			return NormalizeResult(await PerformGetAsync<TrelloCard>(url));

		}

		public async Task<TrelloCard> GetCardByNameAsync(string boardId, string name, bool inclChecklists, bool inclAttachments,
			bool inclMembers)
		{
			var cards = await GetCardsOnBoardAsync(boardId, true);
			var card = cards.FirstOrDefault(c =>
				string.Equals(c.Name, name, StringComparison.CurrentCultureIgnoreCase));

			return card == null ? null : NormalizeResult(await GetCardAsync(card.Id, inclChecklists, inclAttachments, inclMembers));
		}


		public async Task<IEnumerable<TrelloMember>> GetCardMembersAsync(string id)
		{
			var url = BuildUrl($"cards/{id}/members", new Dictionary<string, string>() { { "fields", MEMBER_FIELDS_EXTENDED } });

			return await PerformGetAsync<List<TrelloMember>>(url);
		}


		public async Task<TrelloCard> PatchCardAsync(string id, TrelloCardUpdateDto dto)
		{
			if (dto == null) { throw new ArgumentNullException(nameof(dto)); }

			var url = BuildUrl($"cards/{id}");

			var card = await PerformPutAsync<TrelloCard>(url, dto.Serialize());

			return NormalizeResult(card);
		}

		public async Task<TrelloCard> PatchCardAsync(string id, Dtos.PatchDto patch)
		{
			if (patch == null) { throw new ArgumentNullException(nameof(patch)); }

			var dto = new TrelloCardUpdateDto(patch);

			return await PatchCardAsync(id, dto);
		}

		public async Task<TrelloCard> PatchCardAsync(string id, List<Dtos.PatchDto> patches)
		{
			if (patches == null) { throw new ArgumentNullException(nameof(patches)); }

			var dto = new TrelloCardUpdateDto();
			foreach (var patch in patches)
			{
				dto.ApplyPatch(patch);
			}

			var url = BuildUrl($"cards/{id}");

			var card = await PerformPutAsync<TrelloCard>(url, dto.Serialize());

			return NormalizeResult(card);
		}

		public async Task<TrelloCard> SetCardCover(string id, TrelloCardCover cover)
		{
			if (cover == null)
			{
				return await RemoveCardCover(id);
			}
			//else
			return await PatchCardAsync(id, new TrelloCardUpdateDto() {Cover = cover});
		}

		public async Task<TrelloCard> RemoveCardCover(string id)
		{
			var url = BuildUrl($"cards/{id}");

			var json = "{\"cover\":\"\"}";

			var card = await PerformPutAsync<TrelloCard>(url, json);

			return NormalizeResult(card);
		}


		public async Task<TrelloCard> CreateCardAsync(TrelloCardCreateDto model)
		{
			if (model == null) { throw new ArgumentNullException(nameof(model)); }

			var url = BuildUrl($"cards");

			var card = await PerformPostAsync<TrelloCard>(url, model.Serialize());

			return NormalizeResult(card);
		}

		public async Task<bool> DeleteCardAsync(string id)
		{
			var url = BuildUrl($"cards/{id}");

			var result = await PerformDeleteAsync<object>(url);

			return result != null;
		}

		public async Task<List<TrelloLabel>> AddCardLabelAsync(string id, string labelId)
		{
			var url = BuildUrl($"cards/{id}/idLabels");
			var data = TrelloDtoSerializer.Serialize(new {value = labelId});

			List<string> result = await PerformPostAsync<List<string>>(url, data);

			var card = result == null ? null : (await GetCardAsync(id));

			return card?.Labels;
		}

		public async Task<List<TrelloLabel>> RemoveCardLabelAsync(string id, string labelId)
		{
			var url = BuildUrl($"cards/{id}/idLabels/{labelId}");

			var result = await PerformDeleteAsync<object>(url);

			var card = result == null ? null : (await GetCardAsync(id));

			return card?.Labels;
		}



		//---- STICKER ----
		public async Task<List<TrelloSticker>> GetCardStickersAsync(string id)
		{
			var url = BuildUrl($"cards/{id}/stickers");

			var result = await PerformGetAsync<List<TrelloSticker>>(url);

			return result;
		}

		public async Task<TrelloSticker> AddCardStickerAsync(string id, TrelloStickerBase model)
		{
			var url = BuildUrl($"cards/{id}/stickers");

			var result = await PerformPostAsync<TrelloSticker>(url, model.Serialize());

			return result;
		}



		public async Task<List<TrelloSticker>> RemoveCardStickerAsync(string id, string stickerId)
		{
			var url = BuildUrl($"cards/{id}/stickers/{stickerId}");

			var result = await PerformDeleteAsync<IStickerResult>(url);

			return (result == null || result.Stickers == null) 
				? null 
				: result.Stickers.Select(m => m.ToSticker()).ToList();
		}


		//--- ATTACHMENTS ---
		public async Task<List<TrelloAttachment>> GetCardAttachmentsAsync(string id)
		{
			var url = BuildUrl($"cards/{id}/attachments", new Dictionary<string, string>() { 
				{ "fields", ATTACHMENT_FIELDS }
			});

			var result = await PerformGetAsync<List<TrelloAttachment>>(url);

			return result;

		}

		public async Task<TrelloAttachment> AddCardUrlAttachmentAsync(string id, string url, string name, bool setCover = false)
		{
			var postUrl = BuildUrl($"cards/{id}/attachments");

			var form = new
			{
				name,
				url,
				setCover
			};
			
			return await PerformPostAsync<TrelloAttachment>(postUrl, TrelloDtoSerializer.Serialize(form));
		}

		public async Task<TrelloAttachment> AddCardAttachmentsAsync(string id, TrelloAttachmentCreateDto model)
		{


			var url = BuildUrl($"cards/{id}/attachments");

			if (model.File == null)
			{
				//no file to process
				return await PerformPostAsync<TrelloAttachment>(url, model.Serialize());
			}
			else
			{
				//validate
				try
				{
					ValidateTrelloAttachmentUpload(model);
				}
				catch (ArgumentException e)
				{
					throw new TrelloServiceException("Unable to attach selected file - " + e.Message);
				}


				using var form = new MultipartFormDataContent();

				await using var ms = new MemoryStream();
				await model.File.CopyToAsync(ms);
				using var fileContent = new ByteArrayContent(ms.ToArray());

				fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(model.MimeType);

				//IMPORTANT - note that the form names must be enclosed with quotes.  We escape them here.

				//add the the file form field
				form.Add(fileContent, EncloseInQuotes("file"), EncloseInQuotes(model.File.FileName));

				//add the rest of the content without content type?
				form.Add(new StringContent(model.Name), EncloseInQuotes("name"));
				form.Add(new StringContent(model.SetCover ? "True" : ""), EncloseInQuotes("setCover"));


				return await PerformPostAsync<TrelloAttachment>(url, form);
			}
		}

		public async Task<TrelloAttachment> AddCardAttachmentsAsync(string id, string path, string contentType,
			string name = null, bool setCover = false)
		{
			if (!System.IO.File.Exists(path))
			{
				return null;	//quiet fail
			}

			var url = BuildUrl($"cards/{id}/attachments");

			//validate
			try
			{
				ValidateTrelloAttachmentUpload(path, contentType);
			}
			catch (ArgumentException e)
			{
				throw new TrelloServiceException("Unable to attach selected file - " + e.Message);
			}


			var filename = Path.GetFileName(path);
			name ??= filename;

			using var form = new MultipartFormDataContent();

			await using var stream = System.IO.File.OpenRead(path);
			var streamContent = new StreamContent(stream);
			var file_content = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync());
			file_content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
			file_content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
			{
				FileName = filename,
				Name = name
			};

			//IMPORTANT - note that the form names must be enclosed with quotes.  We escape them here.

			//add the the file form field
			form.Add(file_content, EncloseInQuotes("file"), EncloseInQuotes(filename));

			//add the rest of the content without content type?
			form.Add(new StringContent(name), EncloseInQuotes("name"));
			form.Add(new StringContent(setCover ? "true" : ""), EncloseInQuotes("setCover"));


			return await PerformPostAsync<TrelloAttachment>(url, form);

		}

		protected string EncloseInQuotes(string text)
		{
			return $"\"{text}\"";
		}

		public async Task<bool> RemoveCardAttachmentsAsync(string id, string attachmentId)
		{
			var url = BuildUrl($"cards/{id}/attachments/{attachmentId}");

			await PerformDeleteAsync<object>(url);

			return true;

		}


		protected bool ValidateTrelloAttachmentUpload(TrelloAttachmentCreateDto model)
		{
			if (model == null)
			{
				throw new ArgumentNullException(nameof(model));
			}
			else if (model.File == null || model.File.Length == 0)
			{
				throw new ArgumentException("Missing file");
			}

			var mimeType = attachmentMimeTypes.FirstOrDefault(m =>
				string.Equals(m.Key, model.File.ContentType, StringComparison.CurrentCultureIgnoreCase));
			if (string.IsNullOrEmpty(mimeType.Value))
			{
				throw new ArgumentException("Unsupported file type");
			}

			return true;
		}
		protected bool ValidateTrelloAttachmentUpload(string path, string contentType)
		{
			var mimeType = attachmentMimeTypes.FirstOrDefault(m =>
				string.Equals(m.Key, contentType, StringComparison.CurrentCultureIgnoreCase));
			if (string.IsNullOrEmpty(mimeType.Value))
			{
				throw new ArgumentException("Unsupported file type");
			}
			else if (!FileService.AcceptedExtensions.Any(
				x => path.EndsWith(x, StringComparison.CurrentCultureIgnoreCase)))
			{
				throw new ArgumentException("Unsupported file type");
			}

			return true;
		}

		protected async Task<string> SaveFileLocallyAsync(string cardId, TrelloAttachmentCreateDto model)
		{
			if (model == null)
			{
				throw new ArgumentNullException(nameof(model));
			}
			else if (model.File == null || model.File.Length == 0)
			{
				throw new ArgumentException("Missing file");
			}

			var mimeType = attachmentMimeTypes.FirstOrDefault(m =>
				string.Equals(m.Key, model.File.ContentType, StringComparison.CurrentCultureIgnoreCase));
			if (string.IsNullOrEmpty(mimeType.Value))
			{
				throw new ArgumentException("Unsupported file type");
			}

			var filename = DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss-fff")
			               + mimeType.Value; //include the extension based on mimeType

			var path = Path.Combine(
				Directory.GetCurrentDirectory(),
				"uploads",
				"trello",
				"attachments",
				cardId,
				filename
			);

			var fileService = new FileService();

			var result = await fileService.SaveFormFileAsync(model.File, path);

			return result ? path : null;
		}


		//---- ACTIONS ----

		//actions 
		public async Task<List<TrelloAction>> GetCardActions(string id, string actionType)
		{
			if (TrelloAction.TypeNames().Contains(actionType))
			{
				actionType = TrelloAction.ActionTypes[actionType];
			}

			var url = BuildUrl($"cards/{id}/actions", new Dictionary<string, string>()
			{
				{ "filter", actionType }
			});

			return await PerformGetAsync<List<TrelloAction>>(url);

		}

		public async Task<TrelloAction> AddCardComment(string id, string text)
		{
			var url = BuildUrl($"cards/{id}/actions/comments");
			var json = TrelloDtoSerializer.Serialize<dynamic>(new { text });
			return await PerformPostAsync<TrelloAction>(url, json);
		}


		#endregion


		#region >> LISTS <<



		public async Task<List<TrelloList>> GetListsOnBoardAsync(string id, bool inclClosed, bool inclCards)
		{
			var url = BuildUrl($"boards/{id}/lists", new Dictionary<string, string>()
			{
				{ "filter", inclClosed ? "all" : "open" },
				{ "cards", inclCards ? (inclClosed ? "all" : "open") : "none" },
				{ "card_fields", inclCards ? CARD_FIELDS : "none" }
			});

			var lists = await PerformGetAsync<List<TrelloList>>(url);

			if (lists?.Count > 0)
			{
				lists.ForEach(m =>
				{
					m.Cards = NormalizeResult(m.Cards);
				});
			}

			return lists;
		}


		public async Task<TrelloList> GetListAsync(string id)
		{
			var url = BuildUrl($"lists/{id}", new Dictionary<string, string>() { { "fields", LIST_FIELDS }});

			return await PerformGetAsync<TrelloList>(url);

		}

		public async Task<TrelloList> PatchListAsync(string id, Dtos.PatchDto patch) {
			if (patch == null) { throw new ArgumentNullException(nameof(patch)); }

			var dto = new TrelloListUpdateDto(patch);

			var url = BuildUrl($"lists/{id}");

			var list = await PerformPutAsync<TrelloList>(url, dto.Serialize());

			return list;

		}
		public async Task<TrelloList> PatchListAsync(string id, List<Dtos.PatchDto> patches) {
			if (patches == null) { throw new ArgumentNullException(nameof(patches)); }

			var dto = new TrelloListUpdateDto();
			foreach (var patch in patches)
			{
				dto.ApplyPatch(patch);
			}

			var url = BuildUrl($"lists/{id}");

			var list = await PerformPutAsync<TrelloList>(url, dto.Serialize());

			return list;

		}

		public async Task<TrelloList> CreateListAsync(TrelloListCreateDto model)
		{
			if (model == null)
			{
				throw new ArgumentNullException(nameof(model));
			}

			var url = BuildUrl($"lists");

			var list = await PerformPostAsync<TrelloList>(url, model.Serialize());

			return list;

		}

		#endregion


		#region >> BOARDS <<

		public async Task<bool> BoardExistsAsync(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return false;
			}

			var url = BuildUrl($"boards/{id}", new Dictionary<string, string>() { 
				{ "fields", "id" },
				{ "lists", "none" },
				{ "checklists", "none" },
				{ "customFields", "false" }
			});


			TrelloBoard result;
			try
			{
				result = await PerformGetAsync<TrelloBoard>(url);
			}
			catch (TrelloServiceException e)
			{
				if (e.Details.Any(m => string.Equals(m, "invalid id", StringComparison.CurrentCultureIgnoreCase)))
				{
					result = null;
				}
				else
				{
					throw;
				}
			}
			return result != null;
		}

		public async Task<List<TrelloBoard>> GetMyBoardsAsync(bool detailed)
		{
			var url = BuildUrl($"members/me/boards", new Dictionary<string, string>()
			{
				{"fields", detailed ? BOARD_FIELDS : BOARD_FIELDS_LITE}

			});

			return await PerformGetAsync<List<TrelloBoard>>(url);
		}

		public async Task<TrelloBoard> GetBoardAsync(string id, bool inclChecklists, bool inclCustomFields)
		{
			var url = BuildUrl($"boards/{id}", new Dictionary<string, string>() { { "fields", BOARD_FIELDS },
				{ "lists", "all" },
				{ "checklists", inclChecklists ? "all" : "none" },
				{ "customFields", inclCustomFields.ToString().ToLower() }
			});

			return await PerformGetAsync<TrelloBoard>(url);

		}

		public async Task<TrelloBoard> GetBoardByNameAsync(string name, bool inclChecklists, bool inclCustomFields)
		{
			var url = BuildUrl($"members/me/boards", new Dictionary<string, string>()
			{
				{"fields", ENTITY_FIELDS_MIN}

			});

			var boards = await PerformGetAsync<List<TrelloBoard>>(url);
			var board = boards.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.CurrentCultureIgnoreCase));


			return (board == null) ? null : (await GetBoardAsync(board.Id, inclChecklists, inclCustomFields));
		}

		public async Task<List<TrelloLabel>> GetBoardLabelsAsync(string id)
		{
			var url = BuildUrl($"boards/{id}/labels", new Dictionary<string, string>() { { "fields", BOARD_LABEL_FIELDS }});

			return await PerformGetAsync<List<TrelloLabel>>(url);
		}

		public async Task<List<TrelloChecklist>> GetBoardChecklistsAsync(string id)
		{
			var url = BuildUrl($"boards/{id}/checklists");

			return await PerformGetAsync<List<TrelloChecklist>>(url);
		}

		public async Task<List<TrelloCard>> GetBoardCardsAsync(string id)
		{
			var url = BuildUrl($"boards/{id}/cards");

			return await PerformGetAsync<List<TrelloCard>>(url);
		}


		public async Task<TrelloBoard> CreateBoardFromAsync(string boardSourceId, string orgId, string name,
			string description, bool copyCards = false)
		{
			var url = BuildUrl($"boards", new Dictionary<string, string>()
			{
				{"idBoardSource", boardSourceId},
				{ "idOrganization", orgId },
				{ "keepFromSource", copyCards ? "cards" : "none" },
				{ "powerUps", "all" },
				{"name", name},
				{ "desc", description }
			});

			return await PerformPostAsync<TrelloBoard>(url);
		}


		public async Task<TrelloBoard> PatchBoardAsync(string id, Dtos.PatchDto patch) {
			if (patch == null) { throw new ArgumentNullException(nameof(patch)); }

			var dto = new TrelloBoardUpdateDto(patch);

			var url = BuildUrl($"boards/{id}");

			var card = await PerformPutAsync<TrelloBoard>(url, dto.Serialize());

			return card;

		}
		public async Task<TrelloBoard> PatchBoardAsync(string id, List<Dtos.PatchDto> patches) {
			if (patches == null) { throw new ArgumentNullException(nameof(patches)); }

			var dto = new TrelloBoardUpdateDto();
			foreach (var patch in patches)
			{
				dto.ApplyPatch(patch);
			}

			var url = BuildUrl($"boards/{id}");

			var card = await PerformPutAsync<TrelloBoard>(url, dto.Serialize());

			return card;

		}


		#endregion


		#region >> CUSTOM FIELDS <<

		public async Task<TrelloCustomField> GetCustomFieldAsync(string id)
		{
			var url = BuildUrl($"customFields/{id}");

			return await PerformGetAsync<TrelloCustomField>(url);
		}

		public async Task<IEnumerable<TrelloCustomField>> GetCustomFieldsOnBoardAsync(string id)
		{
			var url = BuildUrl($"boards/{id}/customFields");

			return await PerformGetAsync<IEnumerable<TrelloCustomField>>(url);

		}

		public async Task<IEnumerable<TrelloCardCustomFieldValue>> GetCustomFieldsOnCardAsync(string id,
			bool inclFieldDef)
		{
			var url = BuildUrl($"cards/{id}/customFieldItems");

			var results = await PerformGetAsync<IEnumerable<TrelloCardCustomFieldValue>>(url);

			if (inclFieldDef && results.Any() /* only proceed if there are some fields to lookup */)
			{
				url = BuildUrl($"cards/{id}/customFields");

				var defs = await PerformGetAsync<IEnumerable<TrelloCustomField>>(url);
				
				if (defs != null)
				{
					foreach (var result in results)
					{
						result.CustomField = defs.FirstOrDefault(m => m.Id == result.IdCustomField);

						//need to set the result's value based on it's option value 
						if (result.CustomField?.Options != null)
						{
							result.UpdateValueFromOption();
						}
					}
				}
			}

			return results;

		}

		public async Task<TrelloCardCustomFieldValue> SetCustomFieldOnCardAsync(string id, string customFieldDefId,
			string type,
			string value)
		{
			var url = BuildUrl($"cards/{id}/customField/{customFieldDefId}/item");

			string data = null;

			switch (type.ToLower())
			{
				case "list":
				case "dropdown":
					data = TrelloDtoSerializer.Serialize(new {idValue = value});
					break;
				case "number":
					data = TrelloDtoSerializer.Serialize(new {value = new {number = value}});
					break;
				case "string":
				case "text":
					data = TrelloDtoSerializer.Serialize(new {value = new {text = value}});
					break;
				case "date":
					if (value == null)
					{
						data = TrelloDtoSerializer.Serialize(new {value = ""});	//remove date
					}
					else
					{
						data = TrelloDtoSerializer.Serialize(new {value = new {date = value}});
					}
					break;
				case "checked":
				case "bool":
				case "boolean":
				case "checkbox":
					data = TrelloDtoSerializer.Serialize(new {value = new {@checked = value}});
					break;
			}

			if (string.IsNullOrEmpty(data))
			{
				throw new TrelloServiceException("Invalid custom field type");
			}

			return await PerformPutAsync<TrelloCardCustomFieldValue>(url, data);
		}


		public async Task<TrelloCardCustomFieldValue> SetCustomFieldOnCardAsync(string id,
			IEnumerable<TrelloCustomField> customFieldDefs, TrelloCardCustomFieldLookupDto dto)
		{
			//find the custom field that matches DTO properties
			var field = LookupCustomField(customFieldDefs, dto);

			if (field == null)
			{
				return null;
			}

			//if the custom field is a list (has options), find the value id from the options
			if (field.Options != null && field.Options.Any())
			{
				//find the (first) option where the id or value contains the value in the dto
				TrelloCustomField.CustomFieldOption option;
				if (dto.Value.StartsWith("*"))
				{
					option = field.Options?.FirstOrDefault(m => m.Value.Value.Contains(dto.Value.Substring(1), StringComparison.CurrentCultureIgnoreCase));
				}
				else
				{
					option = field.Options?.FirstOrDefault(m => string.Equals(m.Id, dto.Value, StringComparison.CurrentCultureIgnoreCase));
				}

				if (option != null)
				{
					dto.Value = option.Id;	//set the value to the option id!
				}
			}
			
			return await SetCustomFieldOnCardAsync(id, field.Id, field.Type, dto.Value);

		}

		protected TrelloCustomField LookupCustomField(IEnumerable<TrelloCustomField> customFieldDefs,
			TrelloCardCustomFieldLookupDto dto)
		{
			if (customFieldDefs == null || !customFieldDefs.Any() || dto == null)
			{
				return null;    //quiet fail
			}

			TrelloCustomField field;

			if (dto.Name.StartsWith("*"))
			{
				//find the field with the name that contains the text
				field = customFieldDefs.FirstOrDefault(m =>
					m.Name.Contains(dto.Name.Substring(1), StringComparison.CurrentCultureIgnoreCase));
			}
			else
			{
				//find the field with the exact name
				field = customFieldDefs.FirstOrDefault(m =>
					String.Equals(m.Name, dto.Name, StringComparison.CurrentCultureIgnoreCase));
			}

			return field;

		}

		#endregion


		#region >> ORGANIZATIONS <<

		public async Task<IEnumerable<TrelloOrganization>> GetOrganizationsAsync()
		{
			var url = BuildUrl("members/me/organizations");
			return await PerformGetAsync<IEnumerable<TrelloOrganization>>(url);
		}

		public async Task<TrelloOrganization> GetOrganizationAsync(string id)
		{
			var url = BuildUrl($"organizations/{id}");

			return await PerformGetAsync<TrelloOrganization>(url);

		}

		public async Task<IEnumerable<TrelloBoard>> GetOrganizationBoards(string id)
		{
			var url = BuildUrl($"organizations/{id}/boards");

			return await PerformGetAsync<List<TrelloBoard>>(url);
		}

		public async Task<TrelloOrganization> PatchOrganizationAsync(string id, Dtos.PatchDto patch) {
			if (patch == null) { throw new ArgumentNullException(nameof(patch)); }

			var dto = new TrelloOrganizationUpdateDto(patch);

			var url = BuildUrl($"organizations/{id}");

			var card = await PerformPutAsync<TrelloOrganization>(url, dto.Serialize());

			return card;

		}
		public async Task<TrelloOrganization> PatchOrganizationAsync(string id, List<Dtos.PatchDto> patches) {
			if (patches == null) { throw new ArgumentNullException(nameof(patches)); }

			var dto = new TrelloOrganizationUpdateDto();
			foreach (var patch in patches)
			{
				dto.ApplyPatch(patch);
			}

			var url = BuildUrl($"organizations/{id}");

			var card = await PerformPutAsync<TrelloOrganization>(url, dto.Serialize());

			return card;

		}


		#endregion



		#region >> Member Methods <<

		public async Task<TrelloMember> GetMemberAsync(string id)
		{
			var url = BuildUrl($"members/{id}", new Dictionary<string, string>() { { "fields", MEMBER_FIELDS_EXTENDED }
			});

			return await PerformGetAsync<TrelloMember>(url);

		}
		public async Task<List<TrelloMember>> GetMembersInOrganizationAsync(string id)
		{
			var url = BuildUrl($"organizations/{id}/members", new Dictionary<string, string>() { { "fields", MEMBER_FIELDS }
			});

			return await PerformGetAsync<List<TrelloMember>>(url);
		}


		public async Task<TrelloMember> FindMemberByUsernameAsync(string username, string orgId = null)
		{
			orgId ??= Config.OrganizationId;
			var members =await GetMembersInOrganizationAsync(orgId);
			return members?.FirstOrDefault(m => string.Equals(m.Username, username, StringComparison.CurrentCultureIgnoreCase));
		}


		public async Task<List<TrelloMember>> GetMembersOnCardAsync(string id)
		{
			var url = BuildUrl($"cards/{id}/members", new Dictionary<string, string>() { { "fields", MEMBER_FIELDS }
			});

			return await PerformGetAsync<List<TrelloMember>>(url);


		}

		public async Task<List<TrelloMember>> AddMemberToCardAsync(string cardId, string memberId)
		{
			var url = BuildUrl($"cards/{cardId}/idMembers");
			var content = new { value = memberId };

			var result = await PerformPostAsync<List<TrelloMember>>(url, Serialize(content));

			return result;
		}

		public async Task<List<TrelloMember>> RemoveMemberFromCardAsync(string cardId, string memberId)
		{
			var url = BuildUrl($"cards/{cardId}/idMembers/{memberId}");

			var result = await PerformDeleteAsync<List<TrelloMember>>(url);

			return result;
		}

		public async Task<List<TrelloMember>> EnsureMemberIsOnCardAsync(string cardId, string memberId)
		{
			//adding a member to a card on which they are already a member will result in an error
			//	so we need to first get all members on the card and see if our member is already on it.

			var members = await GetMembersOnCardAsync(cardId);
			if (members == null)
			{
				throw new TrelloServiceException("Unable to get list of members on the card");
			}

			if (members.All(m => m.Id != memberId))
			{
				//add member
				members = await AddMemberToCardAsync(cardId, memberId);
			}

			return members;
		}

		public async Task<List<TrelloCard>> GetAllCardsMemberIsOn(string memberId, bool inclClosed)
		{
			var url = BuildUrl($"members/{memberId}/cards", new Dictionary<string, string>()
			{
				{"filter", inclClosed ? "all" : "visible" }
			});

			var result = await PerformGetAsync<List<TrelloCard>>(url);

			return result;
		}




		public async Task<List<TrelloMember>> GetMembersOnBoardAsync(string id)
		{
			var url = BuildUrl($"boards/{id}/members");

			var members = await PerformGetAsync<List<TrelloMember>>(url);

			if (members == null)
			{
				return null;
			}

			url = BuildUrl($"boards/{id}/memberships");

			var memberships = await PerformGetAsync<List<TrelloMembership>>(url);

			if (memberships != null)
			{
				foreach (var member in members)
				{
					member.Membership = memberships.FirstOrDefault(m => m.IdMember == member.Id);
				}
			}

			return members;
		}

		public async Task<List<TrelloMember>> AddMemberToBoardAsync(string boardId, string memberId,
			TrelloMembership.MemberTypeEnum memberType)
		{
			var url = BuildUrl($"boards/{boardId}/members/{memberId}");
			var content = new {type = memberType.ToString()};

			var result = await PerformPutAsync<TrelloAddMemberToBoardDto>(url, Serialize(content));

			if (result?.Members != null && result.Memberships != null)
			{
				foreach (var member in result.Members)
				{
					member.Membership = result.Memberships.FirstOrDefault(m => m.IdMember == member.Id);
				}
			}
			return result?.Members;
		}

		public async Task<TrelloMember> UpdateMemberOnBoardAsync(string boardId, string memberId,
			TrelloMembership.MemberTypeEnum memberType)
		{
			//get member/membership on board
			var members = await GetMembersOnBoardAsync(boardId);
			var member = members.FirstOrDefault(m => m.Id == memberId);

			if (member == null)
			{
				return null;
			}

			if (member.Membership.MemberType == memberType)
			{
				return member;	//done (no changes required)
			}

			var url = BuildUrl($"boards/{boardId}/memberships/{member.Membership.Id}");
			var content = new {type = memberType.ToString()};

			var result = await PerformPutAsync<TrelloMembershipWithMemberDto>(url, Serialize(content));

			if (result?.MemberType == memberType)
			{
				member.Membership.MemberType = result.MemberType;
			}
			return member;
		}

		public async Task<List<TrelloMember>> RemoveMemberFromBoardAsync(string boardId, string memberId)
		{
			var url = BuildUrl($"boards/{boardId}/members/{memberId}");

			var result = await PerformDeleteAsync<TrelloAddMemberToBoardDto>(url);

			if (result?.Members != null && result.Memberships != null)
			{
				foreach (var member in result.Members)
				{
					member.Membership = result.Memberships.FirstOrDefault(m => m.IdMember == member.Id);
				}
			}
			return result?.Members;
		}


		#endregion




		#region >> HELPERS <<

		protected async Task<T> PerformGetAsync<T>(string url)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			var client = _clientFactory.CreateClient(HTTP_NAME);
			var response = await client.SendAsync(request);

			return await ProcessResponse<T>(response);
		}

		protected async Task<T> PerformPutAsync<T>(string url, string serializedContent = null)
		{
			//serialize the 
			var content = serializedContent == null ? null : new StringContent(serializedContent, Encoding.UTF8, "application/json");

			var client = _clientFactory.CreateClient(HTTP_NAME);
			var response = await client.PutAsync(url, content);

			return await ProcessResponse<T>(response);
		}

		protected async Task<T> PerformPostAsync<T>(string url, string serializedContent = null)
		{
			//serialize the 
			var content = serializedContent == null ? null : new StringContent(serializedContent, Encoding.UTF8, "application/json");
			var client = _clientFactory.CreateClient(HTTP_NAME);
			var response = await client.PostAsync(url, content);

			return await ProcessResponse<T>(response);
		}
		protected async Task<T> PerformPostAsync<T>(string url, MultipartFormDataContent content)
		{
			var client = _clientFactory.CreateClient(HTTP_NAME);
			var response = await client.PostAsync(url, content);

			return await ProcessResponse<T>(response);

		}

		protected async Task<T> PerformPostAsync<T>(string url, string clientName, MultipartFormDataContent content)
		{
			var client = _clientFactory.CreateClient(clientName);
			var response = await client.PostAsync(url, content);

			return await ProcessResponse<T>(response);

		}

		protected async Task<T> PerformDeleteAsync<T>(string url)
		{
			//serialize the 
			var client = _clientFactory.CreateClient(HTTP_NAME);
			var response = await client.DeleteAsync(url);

			return await ProcessResponse<T>(response);
		}

		protected async Task<T> ProcessResponse<T>(HttpResponseMessage response)
		{
			if (response.IsSuccessStatusCode)
			{
				try
				{
					var json = await response.Content.ReadAsStringAsync();
					if (!string.IsNullOrEmpty(json))
					{

						return JsonSerializer.Deserialize<T>(json, SerializerOptions());
					}
				}
				catch (Exception ex)
				{
					throw new TrelloServiceException("Unable to deserialize response", ex);
				}
			}
			else if (response.StatusCode != HttpStatusCode.NotFound)
			{
				//todo: add more information about the response?
				//var requestMessage = response.RequestMessage.Serialize();
				//var requestHeaders = response.RequestMessage.Headers.Serialize();
				//var requestContent = response.RequestMessage.Content.Serialize();

				await ThrowTrelloServiceError(response);
			}

			//else
			return default(T);
		}


		protected TrelloCard NormalizeResult(TrelloCard card)
		{
			//CONVERT TO DATES LOCAL TIME
			if (card != null && card.Due.HasValue)
			{
				card.Due = card.Due.Value.ToLocalTime();
			}

			return card;
		}

		protected List<TrelloCard> NormalizeResult(List<TrelloCard> cards)
		{
			if (cards?.Count > 0)
			{
				return cards.Select(NormalizeResult).ToList();
			}
			//else
			return cards;
		}



		protected string BuildUrl(string path, Dictionary<string, string> queryParams = null)
		{
			if (_config == null)
			{
				throw new TrelloServiceException("Trello service has not been configured with api key and token");
			}
			var url = $"{path}?key={_config.ApiKey}&token={_config.Token}";
			if (queryParams != null)
			{
				foreach (var pair in queryParams)
				{
					url += $"&{pair.Key}={pair.Value}";
				}
			}
			return url;
		}

		protected AuthenticationHeaderValue AuthHeader()
		{
			return AuthenticationHeaderValue.Parse($"OAuth oauth_consumer_key=\"{_config.ApiKey}\", oauth_token=\"{_config.Token}\"");
			//return AuthenticationHeaderValue.Parse($"OAuth oauth_customer_key=\"{_config.ApiKey}\", oauth_token=\"{_config.Token}\"");
		}



		protected string Serialize(object value)
		{
			return TrelloDtoSerializer.Serialize(value);
		}


		protected async Task ThrowTrelloServiceError(HttpResponseMessage response)
		{
			TrelloError terror = null;
			try
			{
				var body = await response.Content.ReadAsStringAsync();
				if (!string.IsNullOrEmpty(body))
				{
					var contentType = response.Content.Headers.FirstOrDefault(m => String.Equals(m.Key, "content-type", StringComparison.CurrentCultureIgnoreCase));
					if (contentType.Value.Any(m => m.Contains("application/json")))
					{
						terror = JsonSerializer.Deserialize<TrelloError>(body, SerializerOptions());
					}
					else if (contentType.Value.Any(m => m.Contains("text/plain")))
					{
						terror = new TrelloError() { Message = body };
					}

				}
			}
			catch (Exception ex)
			{
				terror = new TrelloError() { Message = ex.Message };
			}

			throw new TrelloServiceException($"Problem with Trello Request - {response.StatusCode}", terror)
			{
				Response = response
			};

		}


		protected JsonSerializerOptions SerializerOptions()
		{
			return SerializerHelpers.Options();
		}

		#endregion


	}

}
