using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.Identity
{
    public class ConfirmEmailDto
    {
		[Required, DataType(DataType.EmailAddress)]
		public string Email { get; set; }
		[Required]
		public string Token { get; set; }
		[Required]
		public string Scope { get; set; }
	}
}
