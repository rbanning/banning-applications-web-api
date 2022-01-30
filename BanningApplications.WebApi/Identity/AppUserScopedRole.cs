using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Identity
{
    public class AppUserScopedRole
    {
		[Required, MaxLength(100)]
		public string UserId { get; set; }
		[Required, MaxLength(100)]
		public string Scope { get; set; }
		[Required, MaxLength(100)]
		public string Role { get; set; }

		public string Avatar { get; set; }

		[MaxLength(100)]
		public string TrelloId { get; set; }

		public AppUserScopedRole()
		{
		}
	}
}
