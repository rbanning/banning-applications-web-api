using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.Identity
{
    public class ForgotPasswordDto
    {
		[Required, DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		[Required]
		public string Scope { get; set; }

		//OPTIONAL - if included, user will be sent an password reset link
		//			 if missing/empty, user will be sent a code to allow password reset
		[DataType(DataType.Url), MaxLength(250)]
		public string ResetUrl { get; set; }
	}
}
