using System.Linq;

namespace BanningApplications.WebApi.Services.Trello.Models
{
	public class TrelloCardCustomFieldValue
	{
		public string Id { get; set; }
		public string IdCustomField { get; set; }
		public TrelloCustomField CustomField { get; set; }

		public string IdValue { get; set; }
		public TrelloCustomField.CustomFieldValue Value { get; set; }

		public string IdModel { get; set; }
		public string ModelType { get; set; }


		public TrelloCardCustomFieldValue UpdateValueFromOption()
		{
			if (IdValue != null && CustomField?.Options != null)
			{
				var option = CustomField.Options.FirstOrDefault(m => m.Id == IdValue);
				if (option != null)
				{
					Value = option.Value;
				}
			}

			return this;
		}
	}}
