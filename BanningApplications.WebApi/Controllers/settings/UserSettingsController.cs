using AutoMapper;
using BanningApplications.WebApi.Repo.settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Controllers.settings
{
	[Authorize()]
	[Route("api/settings/user")]
	[ApiController]
    public class UserSettingsController: ApiBaseController
    {
		private readonly IMapper _mapper;
		private readonly IUserSettingsRepository _repo;

		public UserSettingsController(
			IMapper mapper,
			IUserSettingsRepository repo)
		{
			_mapper = mapper;
			_repo = repo;
		}

		[HttpGet]
		public async Task<IActionResult> GetAsync()
		{
			var usr = GetAppUser();
			if (usr == null) { return new UnauthorizedResult(); }

			var settings = await _repo.GetAsync(usr.Id, usr.Scope);

			return new OkObjectResult(_mapper.Map<Dtos.settings.UserSettingsDto>(settings));
		}

		[Authorize(Policy = Identity.Policies.Names.AllowAdmin)]
		[HttpGet("{scope}/{userId}")]
		public async Task<IActionResult> GetAsync([FromRoute] string scope, [FromRoute] string userId)
		{
			var settings = await _repo.GetAsync(userId, scope);

			return new OkObjectResult(_mapper.Map<Dtos.settings.UserSettingsDto>(settings));
		}

		[HttpPost]
		public async Task<IActionResult> CreateOrUpdateAsync([FromBody] Dtos.settings.UserSettingsCreateDto model)
		{
			var usr = GetAppUser();
			if (usr == null) { return new UnauthorizedResult(); }

			if (ModelState.IsValid)
			{
				var settings = await _repo.GetAsync(usr.Id, usr.Scope, true);
				bool isNew = settings == null;

				if (isNew)
				{
					settings = new Entities.settings.UserSettings()
					{
						UserId = usr.Id,
						Scope = usr.Scope,
						Settings = model.Settings
					};
					settings = await _repo.CreateAsync(settings);
				}
				else
				{
					settings.Settings = model.Settings;
					settings = _repo.UpdateEntity(settings);
				}

				await _repo.SaveAsync();

				return new OkObjectResult(_mapper.Map<Dtos.settings.UserSettingsDto>(settings));
			}

			return new BadRequestObjectResult(ModelState);
		}

		[Authorize(Policy = Identity.Policies.Names.AllowAdmin)]
		[HttpPost("{scope}/{userId}")]
		public async Task<IActionResult> CreateOrUpdateAsync([FromRoute] string scope, [FromRoute] string userId, [FromBody] Dtos.settings.UserSettingsCreateDto model)
		{

			if (ModelState.IsValid)
			{
				var settings = await _repo.GetAsync(userId, scope, true);
				bool isNew = settings == null;

				if (isNew)
				{
					settings = new Entities.settings.UserSettings()
					{
						UserId = userId,
						Scope = scope,
						Settings = model.Settings
					};
					settings = await _repo.CreateAsync(settings);
				}
				else
				{
					settings.Settings = model.Settings;
					settings = _repo.UpdateEntity(settings);
				}

				await _repo.SaveAsync();

				return new OkObjectResult(_mapper.Map<Dtos.settings.UserSettingsDto>(settings));
			}

			return new BadRequestObjectResult(ModelState);
		}
	}
}
