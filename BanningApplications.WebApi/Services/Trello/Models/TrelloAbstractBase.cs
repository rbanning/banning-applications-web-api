namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloAbstractBase
    {
		public string Id { get; set; }
		public string Name { get; set; }

		public TrelloAbstractBase()
		{ }

		public TrelloAbstractBase(TrelloAbstractBase model)
		{
			if (model != null)
			{
				Id = model.Id;
				Name = model.Name;
			}
		}
	}
}
