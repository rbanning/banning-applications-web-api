using AutoMapper;
using BanningApplications.WebApi.Dtos.hook;
using BanningApplications.WebApi.Entities.hook;
using BanningApplications.WebApi.Repo.hook;
using BanningApplications.WebApi.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Controllers.hook
{
	[AllowAnonymous]
	[Route("api/hook/hook-request")]
	[ApiController]
    public class HookRequestController: ApiBaseController
    {
		private IMapper _mapper;
		private IHookRequestRepository _repo;

		public HookRequestController(IMapper mapper, IHookRequestRepository repository)
		{
			_mapper = mapper;
			_repo = repository;
		}

		#region >> QUERY ... GETTERS <<

		[HttpGet()]
		public async Task<IActionResult> Get([FromQuery] string auth)
		{
			if (!Authenticate("get", "get", auth))
			{
				return Unauthorized();
			}
			var model = await _repo.GetLast();
			if (model == null) { return NotFound(); }
			return new OkObjectResult(_mapper.Map<HookRequestDto>(model));
		}

		[HttpGet("all")]
		public async Task<IActionResult> GetAll([FromQuery] string auth, [FromQuery] int count = 100)
		{
			if (!Authenticate("get", "getall", auth))
			{
				return Unauthorized();
			}
			var model = await _repo.GetAll(count);
			return new OkObjectResult(_mapper.Map<IEnumerable<HookRequestDto>>(model));
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> Get([FromRoute] string id, [FromQuery] string auth)
		{
			if (!Authenticate("get", "getid", auth))
			{
				return Unauthorized();
			}
			var model = await _repo.Get(id);
			if (model == null) { return NotFound(); }
			return new OkObjectResult(_mapper.Map<HookRequestDto>(model));
		}

		[HttpGet("for/{path}")]
		public async Task<IActionResult> GetFor([FromRoute] string path, [FromQuery] string auth)
		{
			if (!Authenticate("get", "getpath", auth))
			{
				return Unauthorized();
			}
			var model = await _repo.GetAllForPath(path);
			return new OkObjectResult(_mapper.Map<IEnumerable<HookRequestDto>>(model));
		}

		#endregion

		#region >> CREATE ... SETTERS <<

		[HttpHead()]
		public IActionResult Head([FromQuery] string auth)
		{
			return Ok();
		}

		[HttpPost()]
		public async Task<IActionResult> Create([FromQuery] string auth)
		{
			if (!Authenticate("post", "create", auth))
			{
				return Unauthorized();
			}
			var model = await CreateFromRequest("unknown/default path");
			return new OkObjectResult(_mapper.Map<HookRequestDto>(model));
		}

		[HttpHead("{path}")]
		public IActionResult Head([FromRoute] string path, [FromQuery] string auth)
		{
			return Ok();
		}
		[HttpPost("{path}")]
		public async Task<IActionResult> Create([FromRoute] string path, [FromQuery] string auth)
		{
			if (!Authenticate("post", path, auth))
			{
				return Unauthorized();
			}
			var model = await CreateFromRequest(path);
			return new OkObjectResult(_mapper.Map<HookRequestDto>(model));
		}

		private async Task<HookRequest> CreateFromRequest(string path)
		{
			var model = new HookRequest()
			{
				Path = path,
				Headers = _Headers(),
				Body = _Body()
			};

			await _repo.Create(model);
			await _repo.SaveAsync();

			return model;
		}

		private string _Headers()
		{
			return string.Join(" || ", 
				this.HttpContext.Request.Headers
					.Select(m => $"{_Str(m.Key)}={_Str(m.Value)}"));
		}

		private string _Body()
		{
			using (var reader = new StreamReader(Request.Body))
			{
				return reader.ReadToEnd();
			}
		}

		

		private string _Str(string text, string defaultValue = "[null]")
		{
			return (text == null) ? defaultValue : text;
		}

		#endregion


		private bool Authenticate(string method, string path, string auth)
		{
			if (string.IsNullOrEmpty(method) || string.IsNullOrEmpty(auth)) { return false; }

			string code = AuthCode(method, path, auth);

			return string.Equals(code, auth, StringComparison.CurrentCultureIgnoreCase);
		}

		private string AuthCode(string method, string path, string auth)
		{
			if (string.IsNullOrEmpty(method) || string.IsNullOrEmpty(path)) { return null; }

			string code = DateTime.Now.ToString("ddMM");

			switch (method.ToLower())
			{
				case "head":
					code = auth;    //automatic OK
					break;
				case "get":
					code = "please" + code;
					break;
				case "post":
					code = path.ToBase64().Replace("=", "");
					break;
				case "delete":
					code = path.ToBase64().Replace("=", "") + code;
					break;

				default:
					code = null;    //automatic fail
					break;
			}

			return code;
		}

	}
}
