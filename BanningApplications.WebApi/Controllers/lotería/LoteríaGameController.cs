using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Controllers.lotería
{
	[Authorize(Policy = "scope:loteria")]
	[Route("api/lotería/games")]
	[ApiController]
	public class LoteríaGameController: ApiBaseController
    {
		private readonly IMapper _mapper;

		public LoteríaGameController(
			AutoMapper.IMapper mapper)
			:base()
		{
			_mapper = mapper;
		}

		[HttpGet]
		public IActionResult Get()
		{
			return new OkObjectResult(new { result = "ok" });
		}
    }
}
