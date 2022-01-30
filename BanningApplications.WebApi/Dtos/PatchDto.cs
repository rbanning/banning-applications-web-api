using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Dtos
{
    public class PatchDto
    {
		[Required]
		public PatchOperation Op { get; set; }

		[Required, MaxLength(100)]
		public string Path { get; set; }

		public string Value { get; set; }

		public PatchDto()
		{ }

		public PatchDto(PatchOperation op, string path, string value)
		{
			Op = op;
			Path = path;
			Value = value;
		}
	}

	public enum PatchOperation
	{
		replace,
		add,
		remove,
		delete	//alias for remove
	}
}
