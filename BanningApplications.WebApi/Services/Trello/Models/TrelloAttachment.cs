using System;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloAttachment: TrelloAbstractBase
    {

		public DateTime Date { get; set; }
		public bool IsUploaded { get; set; }
		public string MimeType { get; set; }
		public string Url { get; set; }
		public string FileName { get; set; }
	}
}
