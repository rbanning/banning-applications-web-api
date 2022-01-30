using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BanningApplications.WebApi.Services.Trello.Models.TrelloDtos;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloAction
    {
	    public const string ACTION_TYPE_COMMENT = "commentCard";
	    public const string ACTION_TYPE_ADD_ATTACHMENT = "addAttachmentToCard";
	    public const string ACTION_TYPE_UPDATE_CARD = "updateCard";

	    public static Dictionary<string, string> ActionTypes =>
		    new Dictionary<string, string>()
		    {
			    {"comments", ACTION_TYPE_COMMENT},
			    {"attachments", ACTION_TYPE_ADD_ATTACHMENT},
			    {"updates", ACTION_TYPE_UPDATE_CARD},
		    };

	    public static List<string> TypeNames()
	    {
		    return ActionTypes.Keys.ToList();
	    }


	    public string Id { get; set; }
		public dynamic Data { get; set; }

	    public string Type { get; set; }
	    public DateTime Date { get; set; }

		public TrelloMember MemberCreator { get; set; }

		public dynamic DataAsTyped()
		{
			switch (Type)
			{
				case ACTION_TYPE_COMMENT:
					return ConvertData<TrelloActionCommentData>();
				case ACTION_TYPE_ADD_ATTACHMENT:
					return ConvertData<TrelloActionAttachmentData>();
				case ACTION_TYPE_UPDATE_CARD:
					return ConvertData<TrelloActionUpdateCard>();
				default:
					throw new NotImplementedException();
			}
		}

		public T ConvertData<T>() where T: class
		{ 
			try
			{
				var json = TrelloDtoSerializer.Serialize(Data);
				return TrelloDtoSerializer.Deserialize<T>(json);
			}
			catch (Exception)
			{
				return null;
			}
		}

		#region >> Action Data <<

		public abstract class TrelloActionDataAbstract
		{
			public TrelloAbstractBase Card { get; set; }
			public TrelloAbstractBase Board { get; set; }
			public TrelloAbstract List { get; set; }
		}


		public class TrelloActionCommentData : TrelloActionDataAbstract
		{
			public string Text { get; set; }
		}

		public class TrelloActionAttachmentData : TrelloActionDataAbstract
		{
			public TrelloActionAttachmentDataAbstract Attachment { get; set; }

			public class TrelloActionAttachmentDataAbstract: TrelloAbstractBase
			{
				public string Url { get; set; }
			}
		}

		public class TrelloActionUpdateCard : TrelloActionDataAbstract
		{
			public dynamic Old { get; set; }
		}


		public class TrelloActionCreateCommentDto
		{
			[Required, MaxLength(1000)]
			public string Text { get; set; }
		}
		#endregion
	}
}
