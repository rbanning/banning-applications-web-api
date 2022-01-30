using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.Identity
{
    public class ChangePasswordDto
    {
		[Required, DataType(DataType.EmailAddress)]
		public string Email { get; set; }

		[Required, DataType(DataType.Password)]
		public string CurrentPassword { get; set; }

		[Required, DataType(DataType.Password)]
		public string NewPassword { get; set; }

		[Required, DataType(DataType.Password)]
		[Compare("NewPassword")]
		public string ConfirmPassword { get; set; }

	}
}
