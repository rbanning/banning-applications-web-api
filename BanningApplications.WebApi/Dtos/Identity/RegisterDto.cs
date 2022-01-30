using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.Identity
{
    public class RegisterDto
    {

		[Required, MaxLength(100)]
		public string Name { get; set; }

		public string Scope { get; set; }

		[Required, DataType(DataType.EmailAddress), MaxLength(255)]
		public string Email { get; set; }

		[DataType(DataType.PhoneNumber), MaxLength(15)]
		public string Phone { get; set; }

		[Required, DataType(DataType.Password)]
		public string Password { get; set; }

		[Required, DataType(DataType.Password)]
		[Compare("Password")]
		public string ConfirmPassword { get; set; }

		[Required]
		public string Hash { get; set; }

		//OPTIONAL - if included, user will be sent an account activation link
		//			 if missing/empty, user will be sent a code to activate their account
		[DataType(DataType.Url), MaxLength(250)]
		public string ConfirmationUrl { get; set; }


	}
}
