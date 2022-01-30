using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Entities.hook
{
    public class HookRequest
    {
		[Key(), MaxLength(100)]
		public string Id { get; set; }
		public string Path { get; set; }
		public string Headers { get; set; }
		public string Body { get; set; }
		[Required()]
		public DateTime CreatedDate { get; set; }

		public HookRequest()
		{
			this.Id = Guid.NewGuid().ToString("n");
			this.CreatedDate = DateTime.UtcNow;
		}
	}
}
