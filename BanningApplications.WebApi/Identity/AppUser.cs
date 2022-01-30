using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BanningApplications.WebApi.Identity
{
	public class AppUser : IdentityUser
	{
		[Required, MaxLength(100)]
		public string Name { get; set; }

		[NotMapped]
		public string Scope { get; set; }
		[NotMapped]
		public string Role { get; set; }
		[NotMapped]
		public string Avatar { get; set; }
		[NotMapped]
		public string TrelloId { get; set; }

	}
}