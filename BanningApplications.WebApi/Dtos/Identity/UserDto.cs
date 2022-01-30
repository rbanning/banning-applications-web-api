using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.Identity
{
    public class UserDto
    {
		public string Name { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Role { get; set; }
		public string Scope { get; set; }
		public string Avatar { get; set; }
		public string TrelloId { get; set; }
	}

	public class UserWithIdDto: UserDto
	{
		public string Id { get; set; }
	}
}
