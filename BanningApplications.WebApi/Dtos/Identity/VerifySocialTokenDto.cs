using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.Identity
{
	public class VerifySocialTokenDto
	{
		[Required]
		public string Scope { get; set; }
		[Required]
		public string IdToken { get; set; }
		[Required]
		public string Provider { get; set; }
	}
}
