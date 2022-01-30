using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.Identity
{
    public class ConfirmEmailSendAgainDto
    {
		[Required, DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		[Required]
		public string Scope { get; set; }

		//OPTIONAL - if included, user will be sent an account activation link
		//			 if missing/empty, user will be sent a code to activate their account
		[DataType(DataType.Url), MaxLength(250)]
		public string ConfirmationUrl { get; set; }
	}
}
