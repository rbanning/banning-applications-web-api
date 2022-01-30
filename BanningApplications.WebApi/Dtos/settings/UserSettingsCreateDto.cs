using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos.settings
{
    public class UserSettingsCreateDto
    {
        [Required]
		public string Settings { get; set; }
	}
}
