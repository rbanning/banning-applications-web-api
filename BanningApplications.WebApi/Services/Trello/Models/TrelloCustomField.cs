using System.Collections.Generic;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloCustomField: TrelloAbstractBase
    {
	    public string IdModel { get; set; }
	    public string ModelType { get; set; }
	    public string Type { get; set; }	//"checkbox", "list", "date", "text", "number"
	    public List<CustomFieldOption> Options { get; set; }

	    public class CustomFieldOption
	    {
		    public string Id { get; set; }
		    public CustomFieldValue Value { get; set; }
		    public string Color { get; set; }
	    }

	    public class CustomFieldValue
	    {
		    public string Text { get; set; }
		    public string Checked { get; set; }
		    public string Date { get; set; }
		    public string Number { get; set; }

		    public string Value
		    {
			    get
			    {
				    if (!string.IsNullOrEmpty(Checked))
				    {
					    return Checked;
				    } else if (!string.IsNullOrEmpty(Date))
				    {
					    return Date;
				    } else if (!string.IsNullOrEmpty(Number))
				    {
					    return Number;
				    } else if (!string.IsNullOrEmpty(Text))
				    {
					    return Text;
				    }
					//else
					return null;
			    }
		    }
	    }

    }
}
