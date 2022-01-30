using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanningApplications.WebApi.Dtos;
using BanningApplications.WebApi.Services.Trello;
using BanningApplications.WebApi.Services.Trello.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileSystemGlobbing.Internal;

namespace BanningApplications.WebApi.Controllers.Trello
{
	public class TrelloBaseController: ApiBaseController
    {
	    // ReSharper disable once InconsistentNaming
	    protected readonly ITrelloService _trelloService;

	    public TrelloBaseController(ITrelloService trelloService)
	    {
		    _trelloService = trelloService;
	    }

	    #region >> HELPERS <<


	    #endregion

		#region >> CARDS/{id} (Get, Patch, Labels CustomFields, Stickers, Attachments Actions) <<

		[HttpGet("cards/{id}")]
		public async Task<IActionResult> GetCard([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var card = await _trelloService.GetCardAsync(id, true, true, true);
				if (card == null)
				{
					return NotFound();
				}
				//else
				return new OkObjectResult(card);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}


		[HttpGet("cards/{id}/exists")]
		public async Task<IActionResult> CardExists([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.CardExistsAsync(id);
				return new OkObjectResult(result);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}


		[HttpGet("cards/{id}/custom-fields")]
		public async Task<IActionResult> GetCustomFieldsFromCard([FromRoute] string id, [FromQuery] bool inclFieldDefs = true)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var results = await _trelloService.GetCustomFieldsOnCardAsync(id, inclFieldDefs);

				return new OkObjectResult(results);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}


		[HttpPatch("cards/{id}")]
		public async Task<IActionResult> PatchCard([FromRoute] string id, [FromBody] PatchDto patch)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var card = await _trelloService.PatchCardAsync(id, patch);
					if (card == null)
					{
						return NotFound();
					}
					//else
					return new OkObjectResult(card);

				}
				catch (TrelloServiceException tex)
				{
					return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
				}
			}

			//else
			return new BadRequestObjectResult(ModelState);
		}

		[HttpPatch("cards/{id}/multiple")]
		public async Task<IActionResult> PatchCard([FromRoute] string id, [FromBody] List<PatchDto> patches)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var card = await _trelloService.PatchCardAsync(id, patches);
					if (card == null)
					{
						return NotFound();
					}
					//else
					return new OkObjectResult(card);

				}
				catch (TrelloServiceException tex)
				{
					return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
				}
			}

			//else
			return new BadRequestObjectResult(ModelState);
		}


		// --- CUSTOM FIELDS ---

		[HttpPatch("cards/{id}/custom-fields/{customFieldDefId}")]
		public async Task<IActionResult> PatchCustomFieldsFromCard([FromRoute] string id, [FromRoute] string customFieldDefId, [FromBody] PatchDto patch)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				if (patch.Path.StartsWith("/"))
				{
					patch.Path = patch.Path.Substring(1);   //remove the '/'
				}

				var result = await _trelloService.SetCustomFieldOnCardAsync(id, customFieldDefId, patch.Path, patch.Value);

				if (result == null)
				{
					return NotFound();
				}

				//else
				return new OkObjectResult(result);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}


		// --- LABELS ---

		//add a label
		[HttpPost("cards/{id}/labels/{labelId}")]
		public async Task<IActionResult> AddLabelToCard([FromRoute] string id, [FromRoute] string labelId)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.AddCardLabelAsync(id, labelId);

				if (result == null)
				{
					return NotFound();
				}

				//else
				return new OkObjectResult(result);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}

		}

		//remove a label
		[HttpDelete("cards/{id}/labels/{labelId}")]
		public async Task<IActionResult> RemoveLabelToCard([FromRoute] string id, [FromRoute] string labelId)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.RemoveCardLabelAsync(id, labelId);

				if (result == null)
				{
					return NotFound();
				}

				//else
				return new OkObjectResult(result);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}

		}


		// --- STICKERS ---

		[HttpGet("cards/{id}/stickers")]
		public async Task<IActionResult> GetCardStickers([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.GetCardStickersAsync(id);

				if (result == null)
				{
					return NotFound();
				}

				//else
				return new OkObjectResult(result);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}

		[HttpPost("cards/{id}/stickers")]
		public async Task<IActionResult> AddCardSticker([FromRoute] string id, [FromBody] TrelloStickerBase model)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.AddCardStickerAsync(id, model);

				if (result == null)
				{
					return NotFound();
				}

				//else
				return new OkObjectResult(result);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}

		[HttpDelete("cards/{id}/stickers/{stickerId}")]
		public async Task<IActionResult> RemoveCardStickers([FromRoute] string id, [FromRoute] string stickerId)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.RemoveCardStickerAsync(id, stickerId);

				if (result == null)
				{
					return NotFound();
				}

				//else
				return new OkObjectResult(result);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}

		// --- ATTACHMENTS ---

		[HttpGet("cards/{id}/attachments")]
		public async Task<IActionResult> GetCardSAttachments([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.GetCardAttachmentsAsync(id);

				if (result == null)
				{
					return NotFound();
				}

				//else
				return new OkObjectResult(result);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}

		[HttpPost("cards/{id}/attachments")]
		public async Task<IActionResult> AddCardAttachment([FromRoute] string id, [FromBody] TrelloAttachmentCreateDto model)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.AddCardAttachmentsAsync(id, model);

				if (result == null)
				{
					return NotFound();
				}

				//else
				return new OkObjectResult(result);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}
		[HttpPost("cards/{id}/attachments/file")]
		public async Task<IActionResult> AddCardAttachmentFile([FromRoute] string id, [FromForm]TrelloAttachmentCreateDto model, [FromQuery] string run)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
					var result = await _trelloService.AddCardAttachmentsAsync(id, model);
					return new OkObjectResult(result);
				
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}

		[HttpDelete("cards/{id}/attachments/{stickerId}")]
		public async Task<IActionResult> RemoveCardAttachment([FromRoute] string id, [FromRoute] string attachmentId)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.RemoveCardAttachmentsAsync(id, attachmentId);

				if (!result)
				{
					return NotFound();
				}

				//else
				return new OkObjectResult(true);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}


		//-- ACTIONS --
		[HttpGet("cards/{id}/actions")]
		public IActionResult GetCardActionNames([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = TrelloAction.TypeNames();
				return new OkObjectResult(result);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}

		}


		[HttpGet("cards/{id}/actions/{actionType}")]
		public async Task<IActionResult> GetCardActions([FromRoute] string id, [FromRoute] string actionType)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.GetCardActions(id, actionType);
				if (result == null)
				{
					return NotFound();
				}
				//else
				return new OkObjectResult(result);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}

		}

		[HttpPost("cards/{id}/actions/comments")]
		public async Task<IActionResult> AddCardComment([FromRoute] string id, [FromBody] TrelloAction.TrelloActionCreateCommentDto model)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.AddCardComment(id, model.Text);
				if (result == null)
				{
					return NotFound();
				}
				//else
				return new OkObjectResult(result);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}

		}

		#endregion


		#region >> LISTS/{id} (Get, Patch) <<


		[HttpGet("lists/{id}")]
		public async Task<IActionResult> GetList([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var list = await _trelloService.GetListAsync(id);
				if (list == null)
				{
					return NotFound();
				}
				//else
				return new OkObjectResult(list);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}

		[HttpGet("lists/{id}/cards")]
		public async Task<IActionResult> GetCardsFromList([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var cards = await _trelloService.GetCardsOnListAsync(id);
				return new OkObjectResult(cards);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}




		[HttpPatch("lists/{id}")]
		public async Task<IActionResult> PatchList([FromRoute] string id, [FromBody] PatchDto patch)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var list = await _trelloService.PatchListAsync(id, patch);
					if (list == null)
					{
						return NotFound();
					}
					//else
					return new OkObjectResult(list);

				}
				catch (TrelloServiceException tex)
				{
					return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
				}
			}

			//else
			return new BadRequestObjectResult(ModelState);
		}

		[HttpPatch("lists/{id}/multiple")]
		public async Task<IActionResult> PatchList([FromRoute] string id, [FromBody] List<PatchDto> patches)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var list = await _trelloService.PatchListAsync(id, patches);
					if (list == null)
					{
						return NotFound();
					}
					//else
					return new OkObjectResult(list);

				}
				catch (TrelloServiceException tex)
				{
					return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
				}
			}

			//else
			return new BadRequestObjectResult(ModelState);
		}


		#endregion



		#region >> BOARDS/{id} (Get, Patch, CustomFields) <<

		[HttpGet("boards")]
		public async Task<IActionResult> GetBoards([FromQuery] bool detailed = false)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var boards = await _trelloService.GetMyBoardsAsync(detailed);
				if (boards == null)
				{
					return NotFound();
				}
				//else
				return new OkObjectResult(boards);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}

		}

		[HttpGet("boards/{id}")]
		public async Task<IActionResult> GetBoard([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var board = await _trelloService.GetBoardAsync(id, true, true);
				if (board == null)
				{
					return NotFound();
				}
				//else
				return new OkObjectResult(board);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}

		[HttpGet("boards/{id}/exists")]
		public async Task<IActionResult> BoardExists([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.BoardExistsAsync(id);
				return new OkObjectResult(result);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}



		[HttpPost("boards")]
		public async Task<IActionResult> CreateBoardFromTemplateAsync([FromBody] TrelloBoardCreateFromTemplateDto model)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var board = await _trelloService.CreateBoardFromAsync(model.TemplateId, model.OrganizationId, model.Name, model.Description);
				return new OkObjectResult(board);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}

		}


		[HttpGet("boards/{id}/lists")]
		public async Task<IActionResult> GetListsFromBoard([FromRoute] string id, [FromQuery] bool inclClosed = false, [FromQuery] bool inclCards = false)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var lists = await _trelloService.GetListsOnBoardAsync(id, inclClosed, inclCards);
				return new OkObjectResult(lists);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}


		[HttpGet("boards/{id}/cards")]
		public async Task<IActionResult> GeCardsFromBoard([FromRoute] string id, [FromQuery] bool inclClosed = false)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var lists = await _trelloService.GetCardsOnBoardAsync(id, inclClosed);
				return new OkObjectResult(lists);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}


		[HttpGet("boards/{id}/custom-fields")]
		public async Task<IActionResult> GetCustomFieldsFromBoard([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var results = await _trelloService.GetCustomFieldsOnBoardAsync(id);

				return new OkObjectResult(results);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}



		[HttpGet("boards/{id}/checklist")]
		public async Task<IActionResult> GetChecklistsFromBoard([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var results = await _trelloService.GetBoardChecklistsAsync(id);

				return new OkObjectResult(results);
			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}



		[HttpPatch("boards/{id}")]
		public async Task<IActionResult> PatchBoard([FromRoute] string id, [FromBody] PatchDto patch)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var board = await _trelloService.PatchBoardAsync(id, patch);
					if (board == null)
					{
						return NotFound();
					}
					//else
					return new OkObjectResult(board);

				}
				catch (TrelloServiceException tex)
				{
					return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
				}
			}

			//else
			return new BadRequestObjectResult(ModelState);
		}

		[HttpPatch("boards/{id}/multiple")]
		public async Task<IActionResult> PatchBoard([FromRoute] string id, [FromBody] List<PatchDto> patches)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var board = await _trelloService.PatchBoardAsync(id, patches);
					if (board == null)
					{
						return NotFound();
					}
					//else
					return new OkObjectResult(board);

				}
				catch (TrelloServiceException tex)
				{
					return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
				}
			}

			//else
			return new BadRequestObjectResult(ModelState);
		}


		#endregion


		#region >> CUSTOM FIELDS (Get) <<

		[HttpGet("custom-fields/{id}")]
		public async Task<IActionResult> GetCustomFieldAsync([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var result = await _trelloService.GetCustomFieldAsync(id);
				if (result == null)
				{
					return NotFound();
				}
				//else
				return new OkObjectResult(result);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}

		}


		//Custom fields on a board - see BOARDS (boards/{id}/custom-fields)
		//Custom fields on a card - see CARDS (cards/{id}/custom-fields)

		#endregion



		#region >> ORGANIZATIONS/{id} (Get, Patch) <<

		[HttpGet("organizations")]
		public async Task<IActionResult> GetOrganizations()
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var organizations = await _trelloService.GetOrganizationsAsync();
				return new OkObjectResult(organizations);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}

		[HttpGet("organizations/{id}")]
		public async Task<IActionResult> GetOrganization([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var organization = await _trelloService.GetOrganizationAsync(id);
				if (organization == null)
				{
					return NotFound();
				}
				//else
				return new OkObjectResult(organization);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}

		[HttpGet("organizations/{id}/boards")]
		public async Task<IActionResult> GetOrganizationBoards([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var boards = await _trelloService.GetOrganizationBoards(id);
				if (boards == null)
				{
					return NotFound();
				}
				//else
				return new OkObjectResult(boards);

			}
			catch (TrelloServiceException tex)
			{
				return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
			}
		}

		[HttpPatch("organizations/{id}")]
		public async Task<IActionResult> PatchOrganization([FromRoute] string id, [FromBody] PatchDto patch)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var organization = await _trelloService.PatchOrganizationAsync(id, patch);
					if (organization == null)
					{
						return NotFound();
					}
					//else
					return new OkObjectResult(organization);

				}
				catch (TrelloServiceException tex)
				{
					return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
				}
			}

			//else
			return new BadRequestObjectResult(ModelState);
		}

		[HttpPatch("organizations/{id}/multiple")]
		public async Task<IActionResult> PatchOrganization([FromRoute] string id, [FromBody] List<PatchDto> patches)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var organization = await _trelloService.PatchOrganizationAsync(id, patches);
					if (organization == null)
					{
						return NotFound();
					}
					//else
					return new OkObjectResult(organization);

				}
				catch (TrelloServiceException tex)
				{
					return new BadRequestObjectResult(new { reason = tex.Message, details = tex.Details });
				}
			}

			//else
			return new BadRequestObjectResult(ModelState);
		}


		#endregion


		#region >> MEMBERS (Organization, Boards and Cards) <<

		// -- GET AN INDIVIDUAL MEMBER --
		[HttpGet("members/{id}")]
		public async Task<IActionResult> GetMemberAsync([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			var result = await _trelloService.GetMemberAsync(id);
			if (result == null)
			{
				return new NotFoundResult();
			}
			//else
			return new OkObjectResult(result);
		}

		// --- MEMBERS IN ORGANIZATION
		[HttpGet("organizations/{id}/members")]
		public async Task<IActionResult> GetMembersInOrganizationAsync([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			var result = await _trelloService.GetMembersInOrganizationAsync(id);
			if (result == null)
			{
				return new NotFoundResult();
			}
			//else
			return new OkObjectResult(result);
		}

		// --- MEMBERS ON CARDS ---

		[HttpGet("cards/{id}/members")]
		public async Task<IActionResult> GetMembersOnCardAsync([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			var result = await _trelloService.GetMembersOnCardAsync(id);
			if (result == null)
			{
				return new NotFoundResult();
			}
			//else
			return new OkObjectResult(result);
		}

		[HttpPut("cards/{id}/members/{memberId}")]
		public async Task<IActionResult> AddMemberToCardAsync([FromRoute] string id, [FromRoute] string memberId)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			try
			{
				var members = await _trelloService.EnsureMemberIsOnCardAsync(id, memberId);
				return new OkObjectResult(members);
			}
			catch (TrelloServiceException e)
			{
				return new BadRequestObjectResult(new {message = e.Message, details = e.Details});
			}
		}

		[HttpDelete("cards/{id}/members/{memberId}")]
		public async Task<IActionResult> RemoveMemberFromCardAsync([FromRoute] string id, [FromRoute] string memberId)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			//NOTE - RemoveMemberFromCardAsync will throw error if the member does not exists,
			//		 so we get the members first to check
			var members = await _trelloService.GetMembersOnCardAsync(id);
			// ReSharper disable once SimplifyLinqExpressionUseAll
			if (members != null && members.Any(m => m.Id == memberId))
			{
				members = await _trelloService.RemoveMemberFromCardAsync(id, memberId);
			}

			if (members == null)
			{
				return new NotFoundResult();
			}

			//else
			return new OkObjectResult(members);
		}




		// --- MEMBERS ON BOARDS ---

		[HttpGet("boards/{id}/members")]
		public async Task<IActionResult> GetMembersOnBoardAsync([FromRoute] string id)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			var result = await _trelloService.GetMembersOnBoardAsync(id);
			if (result == null)
			{
				return new NotFoundResult();
			}
			//else
			return new OkObjectResult(result);
		}

		[HttpPut("boards/{id}/members/{memberId}")]
		public async Task<IActionResult> AddMemberToBoardAsync([FromRoute] string id, [FromRoute] string memberId, [FromQuery] TrelloMembership.MemberTypeEnum type = TrelloMembership.MemberTypeEnum.observer)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			var result = await _trelloService.AddMemberToBoardAsync(id, memberId, type);
			if (result == null)
			{
				return new NotFoundResult();
			}
			//else
			return new OkObjectResult(result);
		}

		[HttpPatch("boards/{id}/members/{memberId}")]
		public async Task<IActionResult> UpdateMemberToBoardAsync([FromRoute] string id, [FromRoute] string memberId, PatchDto patch)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			if (ModelState.IsValid)
			{
				if (patch.Op != PatchOperation.replace)
				{
					ModelState.AddModelError("Op", "Unsupported patch operation");
				} 
				else if (Enum.TryParse(patch.Value, out TrelloMembership.MemberTypeEnum memberType))
				{
					var result = await _trelloService.UpdateMemberOnBoardAsync(id, memberId, memberType);
					if (result == null)
					{
						return new NotFoundResult();
					}
					//else
					return new OkObjectResult(result);
				}
				else
				{
					ModelState.AddModelError("Value", "Invalid Member Type");
				}
			}

			//else
			return new BadRequestObjectResult(ModelState);
		}

		[HttpDelete("boards/{id}/members/{memberId}")]
		public async Task<IActionResult> RemoveMemberFromBoardAsync([FromRoute] string id, [FromRoute] string memberId)
		{
			//ensure TrelloService is configure using either Controller constructor or DynamicIdentity
			if (!AutoConfigureTrelloServiceIfNeeded())
			{
				return Unauthorized();
			}

			//NOTE - RemoveMemberFromBoardAsync will throw error if member is not on the board,
			//		 so we get the members first to check
			var members = await _trelloService.GetMembersOnBoardAsync(id);
			if (members != null && members.Any(m => m.Id == memberId))
			{
				//remove
				members = await _trelloService.RemoveMemberFromBoardAsync(id, memberId);
			}
			if (members == null)
			{
				return new NotFoundResult();
			}
			//else
			return new OkObjectResult(members);
		}

		#endregion



		#region >> Auto-Configure TrelloService via Authentication >>

		protected bool ConfigureTrelloServiceViaAuthentication()
		{
			//get identity (user and scope)
			var identity = GetUserAndScope();
			if (identity.scope == null || identity.appUser == null)
			{
				return false;
			}
			//configure TrelloService
			var config = TrelloConfig.Find(identity.scope);
			if (config == null) { return false;  }

			_trelloService.Configure(config);

			return _trelloService.IsConfigured;
		}

		#endregion

		#region >> Auto-Configure TrelloService via DynamicIdentity <<

		protected bool AutoConfigureTrelloServiceIfNeeded()
		{
			//first try... Authentication
			if (!_trelloService.IsConfigured)
			{
				var identity = GetUserAndScope();
				if (identity.appUser != null && identity.scope != null)
				{
					var config = TrelloConfig.Find(identity.scope);
					if (config != null)
					{
						_trelloService.Configure(config);
					}
				}
			}

			//second try... use DynamicIdentity
			if (!_trelloService.IsConfigured)
			{
				var identity = GetDynamicIdentity();
				if (identity != null && identity.IsValid)
				{
					var config = TrelloConfig.Find(identity.Scope);
					if (config != null)
					{
						_trelloService.Configure(config);
					}
				}
			}

			//done

			return _trelloService.IsConfigured;
		}

		#endregion
	}
}
