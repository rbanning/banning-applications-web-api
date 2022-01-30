using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanningApplications.WebApi.Dtos;
using BanningApplications.WebApi.Helpers;
using BanningApplications.WebApi.Services.Trello.Models;

namespace BanningApplications.WebApi.Services.Trello
{
    public class TrelloServiceAdvanced
    {
	    // ReSharper disable once InconsistentNaming
	    public ITrelloService Trello { get; set; }

        public TrelloServiceAdvanced(ITrelloService trelloService)
        {
	        this.Trello = trelloService;
        }



        #region >>> Create Card Linking To Another Trello Card <<<


        public async Task<TrelloCard> CreateCardLinkedToCard(string targetBoardId,
	        TrelloCardLinkedCreateDto dto)
        {

	        //validate board 
	        var board = await Trello.GetBoardAsync(targetBoardId, false /*no checklists*/, true /*YES custom fields*/);
	        if (board == null)
	        {
		        throw TrelloServiceAdvancedException.NotFound("Invalid board id");
	        }

	        //validate dto
	        if (dto == null)
	        {
		        throw TrelloServiceAdvancedException.BadRequest("Missing dto");
	        }
			if (string.IsNullOrEmpty(dto.IdList))
	        {
		        throw TrelloServiceAdvancedException.BadRequest("Missing list id");
	        }
			if (string.IsNullOrEmpty(dto.TrelloCardId))
	        {
		        throw TrelloServiceAdvancedException.BadRequest("Missing target card id");
	        }


			//validate list id/name
			TrelloList list;
	        if (dto.IdList.StartsWith("*"))
	        {
		        list = board.Lists.FirstOrDefault(m =>
			        m.Name.Contains(dto.IdList.Substring(1), StringComparison.CurrentCultureIgnoreCase));
	        }
	        else
	        {
				list = await Trello.GetListAsync(dto.IdList);
	        }
	        if (list == null)
	        {
		        throw TrelloServiceAdvancedException.BadRequest("Invalid List");
	        }

	        //validate card id
	        var card = await Trello.GetCardAsync(dto.TrelloCardId);
	        if (card == null)
	        {
		        throw TrelloServiceAdvancedException.BadRequest("Invalid Card Id");
	        }

			var model = dto as TrelloCardCreateDto;
			//update list and board ids from look up results
			model.IdList = list.Id;
			model.IdBoard = board.Id;

			//update label ids (if exist)
			if (model.IdLabels?.Count > 0)
			{
				//convert to valid label ids
				var labels = await Trello.SearchBoardLabels(board.Id, model.IdLabels);
				model.IdLabels = labels.Select(m => m.Id).ToList();
			}

			try
			{
				var result = await Trello.CreateCardAsync(model);

				//add custom fields
				if (dto.CustomFields != null && dto.CustomFields.Any())
				{
					var errors = new List<string>();
					foreach (var customFieldLookupDto in dto.CustomFields)
					{
						try
						{
							await Trello.SetCustomFieldOnCardAsync(result.Id, board.CustomFields, customFieldLookupDto);
						}
						catch (Exception e)
						{
							errors.Add($"- name: {customFieldLookupDto.Name}, value: {customFieldLookupDto.Value}, error: {e.Message}");
						}
					}

					if (errors.Any())
					{
						result = await Trello.PatchCardAsync(result.Id, new TrelloCardUpdateDto()
						{
							Desc = result.Desc + "\n\n**Error trying to set the custom fields**:\n" + string.Join("\n", errors)
						});
					}
				}

				//add the link
				var linkToOriginalCard = Trello.BuildUrlForCard(card.Id);
				var attachment = await Trello.AddCardAttachmentsAsync(result.Id,
					new TrelloAttachmentCreateDto() { Name = card.Name, Url = linkToOriginalCard });

				if (result.Attachments == null)
				{
					result.Attachments = new List<TrelloAttachment>();
				}
				result.Attachments.Add(attachment);

				//add a sticker?
				if (dto.Sticker != null)
				{
					await Trello.AddCardStickerAsync(result.Id, dto.Sticker);
				}

				//add a link on the original card back to this new card
				if (dto.AddLinkOnOriginalCard)
				{
					await Trello.AddCardAttachmentsAsync(card.Id,
						new TrelloAttachmentCreateDto() {Name = result.Name, Url = Trello.BuildUrlForCard(result.Id)});
				}

				//todo: add additional information


				return result;
			}
			catch (TrelloServiceException e)
			{
				throw new TrelloServiceAdvancedException(TrelloServiceAdvancedException.ResponseTypeEnum.ServerError, e);
			}

		}


		#endregion


		//Determine if a card is stale and if so, mark it as stale, otherwise mark it as not stale
		#region >>> "Stale" <<<


		public async Task<TrelloCard> StaleCardAnalyzer(TrelloCard card)
		{
			//HACK
			card = await Trello.GetCardAsync(card.Id);
			return card;
		}


		public bool IsStaleCard(string lastActivity, string due, float daysToStale)
		{
			var lastActivityDate = lastActivity.ParseToDateTimeUtc();
			var dueDate = due.ParseToDateTimeUtc();
			if (lastActivityDate.HasValue)
			{
				return IsStaleCard(lastActivityDate.Value, dueDate, daysToStale);
			}
			//else
			throw new ArgumentException("Invalid 'lastActivity' date", nameof(lastActivity));
		}

		public bool IsStaleCard(DateTime lastActivity, DateTime? due, float daysToStale)
		{
			return IsStaleCard(due ?? lastActivity, daysToStale);
		}

		

		private bool IsStaleCard(DateTime dueOrLastActivity, float daysToStale)
		{
			var diff = DateTime.UtcNow.Subtract(dueOrLastActivity.ToUniversalTime());
			//HACK (check to see if this works)
			return diff.TotalDays > daysToStale;
		}

		#endregion


		//Move card to specific list based on card's due/start date and other logic (min and/or max)
		#region >>> Move Card based on Date <<<

		public async Task<TrelloCard> MoveCardBasedOnDate(
			TrelloCard card,
			string targetListId,
			string min = null,
			string max = null,
			bool useStartIfExists = true,
			string pos = "bottom",
			int offset = 0)
		{
			return await MoveCardBasedOnDate(
				card,
				targetListId,
				min.QueryDateTime(true, true).SetTimeNullable(0 - offset,0),
				max.QueryDateTime(true, true).SetTimeNullable(23 - offset,59,59), useStartIfExists,
				pos);
		}
		public async Task<TrelloCard> MoveCardBasedOnDate(
			TrelloCard card,
			string targetListId,
			DateTime? min = null,
			DateTime? max = null,
			bool useStartIfExists = true,
			string pos = "bottom")
		{
			//checking
			if (card == null)
			{
				return null;
			} 

			//bypass checks if any of these are true!
			if (string.IsNullOrEmpty(targetListId)						//must have a target list id
			    || card.DueComplete == true								//cannot be completed
			    || (!card.Due.HasValue && !card.Start.HasValue)			//must have a due or start date
			    || string.Equals(card.IdList, targetListId))		//cannot already be in the target list
			{
				return null;
			}

			//only check on those conditions provided
			var checks = new List<bool>();
			if (min.HasValue)
			{
				checks.Add(MinDateTime(card, useStartIfExists) >= min);
			}

			if (max.HasValue)
			{
				checks.Add(MaxDateTime(card, useStartIfExists) <= max);
			}

			return await MoveCardBasedOnDate(card, targetListId, checks, pos);
		}

		public async Task<TrelloCard> MoveCardBasedOnDate(
			TrelloCard card,
			string targetListId,
			List<bool> checks,
			string pos = "bottom")
		{
			//only perform move if all of the checks are true
			if (checks?.Count > 0 && checks.TrueForAll(m => m))
			{
				var patches = new List<PatchDto>()
				{
					new PatchDto(PatchOperation.replace, "/idList", targetListId),
					new PatchDto(PatchOperation.replace, "/pos", pos)
				};

				return await Trello.PatchCardAsync(card.Id, patches);
			}

			//else - nothing to do
			return null;
		}


		protected DateTime? MinDateTime(TrelloCard card, bool useStartIfExists, bool convertToUtc = true)
		{
			var result = DateExtensions.Min(card.Due, useStartIfExists ? card.Start : null);
			if (result.HasValue && convertToUtc)
			{
				return result.Value.ToUniversalTime();
			}
			//else
			return result;
		}
		protected DateTime? MaxDateTime(TrelloCard card, bool useStartIfExists, bool convertToUtc = true)
		{
			var result = DateExtensions.Min(card.Due, useStartIfExists ? card.Start : null);
			if (result.HasValue && convertToUtc)
			{
				return result.Value.ToUniversalTime();
			}
			//else
			return result;
		}


		#endregion


		#region >> HELPERS <<


		#endregion


		public class TrelloServiceAdvancedException : TrelloServiceException
        {
	        public enum ResponseTypeEnum
	        {
                NotFound,
                BadRequest,
                ServerError
	        }

	        public ResponseTypeEnum ResponseType { get; set; }

	        public TrelloServiceAdvancedException(ResponseTypeEnum type, string message)
		        : base(message)
	        {
		        ResponseType = type;
	        }
	        public TrelloServiceAdvancedException(ResponseTypeEnum type, string message, List<string> details)
		        : base(message, details)
	        {
		        ResponseType = type;
	        }

			public TrelloServiceAdvancedException(ResponseTypeEnum type, string message, Exception inner)
		        : base(message, inner)
	        {
		        ResponseType = type;
	        }

	        public TrelloServiceAdvancedException(ResponseTypeEnum type, string message, TrelloError error)
		        : base(message, error)
	        {
		        ResponseType = type;
	        }

			public TrelloServiceAdvancedException(ResponseTypeEnum type, string message, Exception inner, List<string> details)
		        : base(message, inner, details)
	        {
		        ResponseType = type;
	        }

			public TrelloServiceAdvancedException(ResponseTypeEnum type, string message, TrelloError error, List<string> details)
		        : base(message, error, details)
	        {
		        ResponseType = type;
	        }
			public TrelloServiceAdvancedException(ResponseTypeEnum type, TrelloServiceException trelloServiceException)
		        : base(trelloServiceException.Message, trelloServiceException.InnerException)
	        {
		        ResponseType = type;
		        Details = trelloServiceException.Details;
	        }


			public static TrelloServiceAdvancedException NotFound(string message)
			{
				return new TrelloServiceAdvancedException(ResponseTypeEnum.NotFound, message);
			}
			public static TrelloServiceAdvancedException BadRequest(string message, TrelloError error = null)
			{
				return new TrelloServiceAdvancedException(ResponseTypeEnum.BadRequest, message, error: error);
			}
		}
	}
}
