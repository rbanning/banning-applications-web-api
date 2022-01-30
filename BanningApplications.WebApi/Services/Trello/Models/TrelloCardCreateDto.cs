using System;
using System.Collections.Generic;
using BanningApplications.WebApi.Helpers;
using BanningApplications.WebApi.Services.Trello.Models.TrelloDtos;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloCardCreateDto
    {
		public string Name { get; set; }
		public string Desc { get; set; }
		public bool? Closed { get; set; }
		public string IdList { get; set; }
		public string IdBoard { get; set; }
		public DateTime? Due { get; set; }
		public bool? DueComplete { get; set; }
		public string Address { get; set; }
		public string LocationName { get; set; }
		public string Coordinates { get; set; }		//latitude,longitude
		public string Pos { get; set; }				//"top" or "bottom"

		//added 2021-11-09
		public List<string> IdLabels { get; set; }
		public string DueCalc { get; set; }


		public string Serialize()
		{
			//update Due property only if Due is empty and DueCalc has a value
			if (!Due.HasValue && !string.IsNullOrEmpty(DueCalc))
			{
				Due = DueCalc.QueryDateTime();
			}
			Console.WriteLine("Updated Due {0} from DueCalc {1}", Due.HasValue ? Due.ToString() : "null", DueCalc);

			return TrelloDtoSerializer.Serialize(this);
		}
	}
}
