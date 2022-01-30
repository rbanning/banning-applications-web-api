namespace BanningApplications.WebApi.Services.Trello.Models
{
    public abstract class TrelloAbstract: TrelloAbstractBase
    {
		public string Desc { get; set; }
		public bool Closed { get; set; }
		public string Url { get; set; }

		protected TrelloAbstract()
		{ }

		protected TrelloAbstract(TrelloAbstract model)
			:base(model)
		{
			if (model != null)
			{
				Desc = model.Desc;
				Closed = model.Closed;
				Url = model.Url;
			}
		}
	}
}
