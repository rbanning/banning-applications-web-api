using BanningApplications.WebApi.Identity;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using BanningApplications.WebApi.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BanningApplications.WebApi.Controllers
{

	//TODO - See trg-database-app issued #22 for some fixes



	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ApiBaseController
	{
		private readonly IMapper _mapper;
		private readonly IConfiguration _config;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly UserManager<AppUser> _userManager;
		private readonly IAppUserScopedRolesRepository _scopedRolesRepo;
		private readonly IAppUserTokenRepository _tokenRepo;

		public AuthController(IMapper mapper, 
			IConfiguration config,
			IHttpClientFactory httpClientFactory,
			UserManager<AppUser> userManager,
			IAppUserScopedRolesRepository scopedRolesRepo,
			IAppUserTokenRepository tokenRepo)
		{
			_mapper = mapper;
			_config = config;
			_httpClientFactory = httpClientFactory;
			_userManager = userManager;
			_scopedRolesRepo = scopedRolesRepo;
			_tokenRepo = tokenRepo;
		}

		#region >> User Info <<

		[Authorize]
		[HttpGet("echo")]
		public IActionResult Echo()
		{
			var header = HttpContext.Request.Headers.FirstOrDefault(m => m.Key == "Authorization");
			return new OkObjectResult(header);
		}

		[Authorize]
		[HttpGet("user")]
		public IActionResult GetUserFromToken()
		{
			var user = GetAppUser();
			if (user == null) { return new BadRequestObjectResult("Unable to get app user"); }

			//else
			return new OkObjectResult(_mapper.Map<Dtos.Identity.UserDto>(user));
		}

		[Authorize]
		[HttpGet("user/id")]
		public IActionResult GetUserIdFromToken()
		{
			var user = GetAppUser();
			if (user == null) { return new BadRequestObjectResult("Unable to get app user"); }

			//else
			return new OkObjectResult(new { id = user.Id });
		}

		#endregion

		#region >> Avatars <<

		[Authorize]
		[HttpGet("avatars")]
		public IActionResult Avatars()
		{
			var repo = new AvatarRepo();
			return new OkObjectResult(repo.Avatars);
		}


		[Authorize]
		[HttpPut("avatars")]
		public async Task<IActionResult> UpdateAvatar([FromBody] Dtos.Identity.UpdateAvatarDto model)
		{
			//initial validation
			if (ModelState.IsValid)
			{
				var user = GetAppUser();
				if (user == null) { return new BadRequestObjectResult("Unable to get app user"); }

				if (!RegisteredScopes.IsValid(model.Scope) || !string.Equals(model.Scope, user.Scope))
				{
					//verify scope
					ModelState.AddModelError("", "Invalid Request");
				}
				else
				{
					if (string.IsNullOrEmpty(model.Avatar))
					{
						model.Avatar = new AvatarRepo().RandomAvatar();
					}
					var scopedUser = await _scopedRolesRepo.UpdateAvatarAsync(user.Id, user.Scope, model.Avatar);
					user.Avatar = scopedUser.Avatar;

					return new OkObjectResult(_mapper.Map<Dtos.Identity.UserDto>(user));
				}
			}

			//else
			return BadRequest(ModelState);
		}


		#endregion

		#region >> Authenticate (CreateToken and RefreshToken) <<

		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> CreateToken([FromBody] Dtos.Identity.LoginDto model)
		{
			if (ModelState.IsValid)
			{
				if (RegisteredScopes.IsValid(model.Scope))
				{
					var user = await _userManager.FindByEmailAsync(model.Email);  //find by email

					if (user != null)
					{
						if (await _userManager.IsLockedOutAsync(user))
						{
							return new BadRequestObjectResult(new AuthLoginFailed(AuthLoginFailed.FailReason.lockout));
						}

						var scopedRole = await _scopedRolesRepo.FindAsync(user.Id, model.Scope);

						if (scopedRole != null)
						{
							//checkPassword will fail if email has not been confirmed, so must do it first
							if (!(await _userManager.IsEmailConfirmedAsync(user)))
							{
								return new BadRequestObjectResult(new AuthLoginFailed(AuthLoginFailed.FailReason.not_activated));
							}
							else if (await _userManager.CheckPasswordAsync(user, model.Password))
							{
								await _userManager.ResetAccessFailedCountAsync(user);

								// await HttpContext.SignInAsync(new ClaimsPrincipal(user.ToClaimsIdentity()));

								user.Scope = scopedRole.Scope;
								user.Role = scopedRole.Role;
								user.Avatar = scopedRole.Avatar;
								user.TrelloId = scopedRole.TrelloId;
								return new OkObjectResult(TokenResult(user));
							}
							else
							{
								await _userManager.AccessFailedAsync(user);
							}

						}

					}
				}

				//else (generic error message)
				return new BadRequestObjectResult(new AuthLoginFailed(AuthLoginFailed.FailReason.invalid));
			}

			return BadRequest(ModelState);
		}

		[Authorize]
		[HttpPost("refresh")]
		public async Task<IActionResult> RefreshToken()
		{
			var who = GetAppUser();
			if (who != null)
			{
				var user = await _userManager.FindByEmailAsync(who.Email);
				if (user != null && string.Equals(who.SecurityStamp, user.SecurityStamp))
				{
					var scopedRole = await _scopedRolesRepo.FindAsync(user.Id, who.Scope);
					if (scopedRole != null)
					{
						user.Scope = scopedRole.Scope;
						user.Role = scopedRole.Role;
						user.Avatar = scopedRole.Avatar;
						user.TrelloId = scopedRole.TrelloId;
						return new OkObjectResult(TokenResult(user));
					} else
					{
						return new BadRequestObjectResult("Unable to refresh token - invalid scope");
					}
				}

			}

			return new BadRequestObjectResult("Token has expired");
		}



		private object TokenResult(AppUser user)
		{
			return new
			{
				token = user.ToAuthToken(_config),
				expires = _config.AuthExpires(),
				user = _mapper.Map<Dtos.Identity.UserDto>(user)
			};
		}

		#endregion

		#region >> GOOGLE - verify idToken <<

		[AllowAnonymous]
		[HttpPost("verify-social-signin")]
		public async Task<IActionResult> VerifySocialSignin([FromBody] Dtos.Identity.VerifySocialTokenDto model)
		{
			if (ModelState.IsValid)
			{
				var scope = RegisteredScopes.Find(model.Scope);
				if (scope == null) { ModelState.AddModelError("", "Invalid request"); }
				else
				{
					var verifier = new SocialSigninValidator(_httpClientFactory);
					switch (model.Provider.ToLower())
					{
						case "google":
							if (scope.AllowGoogleSignin)
							{
								var result = await verifier.VerifyGoogleSignin(model.IdToken, scope.SupportedGoogleClientIds);
								if (!result.IsValid)
								{
									return new BadRequestObjectResult(result);
								}

								try
								{
									var user = await LoginSocial(result.UserInfo, scope);
									if (user == null) { ModelState.AddModelError("", "Unknown user"); }
									else
									{
										return new OkObjectResult(TokenResult(user));
									}
								}
								catch (Exception)
								{
									return new BadRequestObjectResult("unable to complete operation");
								}
								
							}
							else
							{
								ModelState.AddModelError("Provider", "Signin from this provider is not configured");
							}
							break;
						default:
							ModelState.AddModelError("Provider", "Unsupported provider");
							break;
					}

				}
			}

			//else
			return new BadRequestObjectResult(ModelState);
		}

		private async Task<AppUser> LoginSocial(SocialUserDetails model, RegisteredScopes.Scope scope)
		{
			if (model == null) { throw new ArgumentNullException("model"); }
			if (scope == null) { throw new ArgumentNullException("scope"); }

			var user = await _userManager.FindByNameAsync(model.Email);
			if (user == null)
			{
				user = await LoginSocialCreateUser(model, scope);
			}
			else if (!await LoginSocialAddScopedRole(user, scope))
			{
				user = null;
			}

			return user;
		}

		private async Task<AppUser> LoginSocialCreateUser(SocialUserDetails model, RegisteredScopes.Scope scope)
		{
			if (model == null) { throw new ArgumentNullException("model"); }
			if (scope == null) { throw new ArgumentNullException("scope"); }

			AppUser user = null;
			if (scope.AllowAutoRegisterUser)
			{
				user = new AppUser()
				{
					Id = Guid.NewGuid().ToString("n"),
					UserName = model.Email.ToLower(),
					Email = model.Email,
					Name = model.Name,
					Role = RegisteredRoles.viewer,
					Scope = scope.Id,
					EmailConfirmed = true
				};

				var result = await _userManager.CreateAsync(user);
				if (result.Succeeded)
				{
					await _scopedRolesRepo.CreateOrUpdateAsync(user.Id, user.Scope, user.Role, new AvatarRepo().RandomAvatar());
				}
				else
				{
					throw new Exception(string.Join(" | ", result.Errors.Select(m => m.Description)));
				}

			}

			return user;
		}

		private async Task<bool> LoginSocialAddScopedRole(AppUser user, RegisteredScopes.Scope scope)
		{
			if (user == null) { throw new ArgumentNullException("user"); }
			if (scope == null) { throw new ArgumentNullException("scope"); }

			var scopedRole = await _scopedRolesRepo.FindAsync(user.Id, scope.Id);
			if (scopedRole == null && scope.AllowAutoRegisterUser)
			{
				scopedRole = await _scopedRolesRepo.CreateOrUpdateAsync(user.Id, user.Scope, user.Role, new AvatarRepo().RandomAvatar());
			}
			
			if (scopedRole != null)
			{
				user.Scope = scopedRole.Scope;
				user.Role = scopedRole.Role;
				return true;
			}

			return false;
		}

		#endregion

		#region >> REGISTER <<

		[AllowAnonymous]
		[HttpPost("user-hash")]
		public IActionResult RegistrationHash([FromBody] Dtos.Identity.RegisterDto model)
		{
			//initial validation
			if (ModelState.IsValid)
			{
				var scope = RegisteredScopes.Find(model.Scope);

				if (scope == null || !string.Equals(scope.Secret, model.Hash))
				{
					//verify scope
					ModelState.AddModelError("", "Invalid Request");
				}
				else
				{
					var hash = CalculateAuthHash(model.Email, model.Name, model.Scope);
					return new OkObjectResult(new {hash });
				}
			}

			//else
			return BadRequest(ModelState);
		}


		[AllowAnonymous]
		[HttpPost("user")]
		public async Task<IActionResult> Register([FromBody] Dtos.Identity.RegisterDto model)
		{
			//initial validation
			if (ModelState.IsValid)
			{
				var scope = RegisteredScopes.Find(model.Scope);
				if (scope == null)
				{
					//verify scope
					ModelState.AddModelError("", "Invalid Request");
				}
				else if (!string.Equals(model.Hash, CalculateAuthHash(model.Email, model.Name, model.Scope)))
				{
					//verify hash
					ModelState.AddModelError("", "Invalid Request");
				}
				else
				{
					Services.Email.EmailResponse emailResponse = null;

					string avatar = new AvatarRepo().RandomAvatar();

					var user = await _userManager.FindByNameAsync(model.Email);
					if (user == null)
					{
						//NEW USER
						user = new AppUser()
						{
							Id = Guid.NewGuid().ToString("n"),
							UserName = model.Email.ToLower(),
							Email = model.Email.ToLower(),
							PhoneNumber = model.Phone,
							Name = model.Name,
							EmailConfirmed = !scope.RequireEmailValidation,
							Role = RegisteredRoles.viewer,
							Scope = model.Scope,
							Avatar = avatar
						};

						var result = await _userManager.CreateAsync(user, model.Password);
						if (result.Succeeded)
						{
							await _scopedRolesRepo.CreateOrUpdateAsync(user.Id, user.Scope, user.Role, avatar);

							if (scope.RequireEmailValidation)
							{
								emailResponse = await SendAccountActivationEmailAsync(user, model.ConfirmationUrl);
							}

							return new OkObjectResult(new AuthRegistrationOk(_mapper.Map<Dtos.Identity.UserDto>(user), user.EmailConfirmed, emailResponse));
						}
						else
						{
							return new BadRequestObjectResult(result.Errors.Select(m => m.Description));
						}
					}
					else
					{
						//user already exists -
						var scopedRole = await _scopedRolesRepo.FindAsync(user.Id, model.Scope);
						if (scopedRole == null)
						{
							//user is new to the scope, so add
							//ISSUE #12 - do NOT update any information about the user (incl. password or name)
							//			- just assign basic (viewer) role and random avatar
							user.Scope = model.Scope;
							user.Role = RegisteredRoles.viewer;
							user.Avatar = avatar;
							await _scopedRolesRepo.CreateOrUpdateAsync(user.Id, model.Scope, user.Role, user.Avatar);

							if (!user.EmailConfirmed)
							{
								emailResponse = await SendAccountActivationEmailAsync(user, model.ConfirmationUrl);
							}

							return new OkObjectResult(new AuthRegistrationOk(_mapper.Map<Dtos.Identity.UserDto>(user), user.EmailConfirmed, emailResponse));

						}

						//else (already exists in scope)
						user.Scope = scopedRole.Scope;
						user.Role = scopedRole.Role;
						user.Avatar = scopedRole.Avatar;
						if (!user.EmailConfirmed)
						{
							emailResponse = await SendAccountActivationEmailAsync(user, model.ConfirmationUrl);
						}

						return new BadRequestObjectResult(new AuthRegistrationDuplicate(user.EmailConfirmed, emailResponse));

					}

				}
			}

			//else
			return BadRequest(ModelState);
		}



		[AllowAnonymous]
		[HttpPost("confirm/email")]
		public async Task<IActionResult> ConfirmEmail([FromBody] Dtos.Identity.ConfirmEmailDto model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByNameAsync(model.Email);
				if (user != null)
				{
					if (model.Token.Length == _tokenRepo.KnownUserTokenLength(KnownUserToken.confirm_email))
					{
						//use AppUserToken repo to validate
						var success = await _tokenRepo.ValidateAndDeleteAsync(user.Id, model.Scope, KnownUserToken.confirm_email, model.Token);
						if (success)
						{
							user.EmailConfirmed = true;
							await _userManager.UpdateAsync(user);
							return new OkObjectResult(new { success = true });
						}
					}
					else
					{
						var result = await _userManager.ConfirmEmailAsync(user, model.Token);
						if (result.Succeeded)
						{
							return new OkObjectResult(new { success = true });
						}
					}

				}				
			}

			//else
			if (ModelState.IsValid) { return new BadRequestObjectResult("Invalid Email and/or Token"); }
			return BadRequest(ModelState);
		}


		[AllowAnonymous]
		[HttpPost("confirm/email/resend")]
		public async Task<IActionResult> ConfirmEmailResend([FromBody] Dtos.Identity.ConfirmEmailSendAgainDto model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByNameAsync(model.Email);
				if (user == null)
				{
					ModelState.AddModelError("email", "invalid email");
				}
				else
				{
					user.Scope = model.Scope;
					var response = await SendAccountActivationEmailAsync(user, model.ConfirmationUrl);
					return new OkObjectResult(response);
				}
			}

			//else
			return BadRequest(ModelState);
		}


		#endregion

		#region >> UPDATE <<

		[HttpPatch("user")]
		[Authorize()]
		public async Task<IActionResult> UpdateUser([FromBody] Dtos.PatchDto model)
		{
			//initial validation
			if (ModelState.IsValid)
			{
				var who = GetAppUser();
				if (who == null) { return new BadRequestObjectResult("Unable to get app user"); }
				var user = await _userManager.FindByEmailAsync(who.Email);
				if (user == null || !string.Equals(who.SecurityStamp, user.SecurityStamp))
				{
					return NotFound();
				}

				switch (model.Op)
				{
					case Dtos.PatchOperation.replace:
						//used for changing email and phone
						string token;    
						IdentityResult result;	

						switch (model.Path.ToLower())
						{
							case "/name":
								user.Name = model.Value;
								result = await _userManager.UpdateAsync(user);
								if (!result.Succeeded)
								{
									return new BadRequestObjectResult(result.Errors);
								}

								break;

							case "/email":
								var email = model.Value.ToLower();
								token = await _userManager.GenerateChangeEmailTokenAsync(user, email);
								result = await _userManager.ChangeEmailAsync(user, email, token);
								if (result.Succeeded)
								{
									user.UserName = email;
									result = await _userManager.UpdateAsync(user);
								}
								if (result.Succeeded)
								{
									await _userManager.UpdateNormalizedUserNameAsync(user);
								}

								if (!result.Succeeded)
								{
									return new BadRequestObjectResult(result.Errors);
								}

								break;

							case "/phonenumber":
							case "/phone":
								token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.Value);
								result = await _userManager.ChangePhoneNumberAsync(user, model.Value, token);
								if (!result.Succeeded)
								{
									return new BadRequestObjectResult(result.Errors);
								}
								break;

							case "/avatar":
								if (string.IsNullOrEmpty(model.Value))
								{
									model.Value = new AvatarRepo().RandomAvatar();
								}
								var scopedUserForAvatar = await _scopedRolesRepo.UpdateAvatarAsync(user.Id, who.Scope, model.Value);
								user.Avatar = scopedUserForAvatar.Avatar;
								break;

							case "/trelloid":
								var scopedUserForTrelloId = await _scopedRolesRepo.UpdateAvatarAsync(user.Id, who.Scope, model.Value);
								user.TrelloId = scopedUserForTrelloId.TrelloId;
								break;

							default:
								return new BadRequestObjectResult("Unsupported patch path: " + model.Path);

						}

						//fill in the blanks
						user.Scope = who.Scope;
						user.Role = who.Role;
						return new OkObjectResult(_mapper.Map<Dtos.Identity.UserDto>(user));

					//case Dtos.PatchOperation.add:
					//case Dtos.PatchOperation.remove:
					//case Dtos.PatchOperation.delete:
					default:
						return new BadRequestObjectResult("Unsupported patch operation: " + model.Op.ToString());
				}

			}

			//else
			return BadRequest(ModelState);

		}

		#endregion


		#region >> PASSWORD <<

		[AllowAnonymous]
		[HttpPost("password/forgot")]
		public async Task<IActionResult> ForgotPassword([FromBody] Dtos.Identity.ForgotPasswordDto model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				
				if (user != null)
				{
					user.Scope = model.Scope;
					var response = await SendPasswordResetEmailAsync(user, model.ResetUrl);

					return new OkObjectResult(new { email = user.Email, response });

				}
				else
				{
					//email the user and tell them they do not have an account
					//"maybe you used a different email address to register for the site?"

					//this prevents phishing for email addresses / accounts
					return new OkObjectResult(new { email = model.Email });
				}
			}

			//else
			return BadRequest(ModelState);

		}

		[AllowAnonymous]
		[HttpPost("password/reset")]
		public async Task<IActionResult> ResetPassword([FromBody] Dtos.Identity.ResetPasswordDto model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				
				if (user != null)
				{
					var errors = await ValidatePasswordStrengthAsync(user, model.Password);
					if (errors != null)
					{
						return new BadRequestObjectResult(errors);
					}

					var success = await ResetPasswordAsync(user, model.Scope, model.Password, model.Token);

					if (success)
					{
						//if the user's email had not yet been confirmed, then let's mark it as confirmed (since the got the token via email)
						if (!await _userManager.IsEmailConfirmedAsync(user))
						{
							//manually update the user (since we do not have an email confirmation token
							user.EmailConfirmed = true;
							await _userManager.UpdateAsync(user);
						}

						//if the user was locked out, reset their lockout date/time so they can now log in
						if (await _userManager.IsLockedOutAsync(user))
						{
							await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
						}

						//done
						return new OkObjectResult(new { success = true });
					}
				}

				ModelState.AddModelError("", "Invalid Request");
			}

			//else
			return BadRequest(ModelState);

		}


		[Authorize]
		[HttpPost("password/change")]
		public async Task<IActionResult> ChangePassword([FromBody] Dtos.Identity.ChangePasswordDto model)
		{
			if (ModelState.IsValid)
			{
				var who = GetAppUser();
				if (who == null || !string.Equals(who.Email, model.Email, StringComparison.CurrentCultureIgnoreCase))
				{
					return NotFound();
				}

				var user = await _userManager.FindByEmailAsync(model.Email);
				
				if (user != null)
				{
					var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

					if (result.Succeeded)
					{
						return new OkObjectResult(new { success = true });
					}
					else
					{
						foreach (var err in result.Errors)
						{
							ModelState.AddModelError("", err.Description);
						}
					}

				} else
				{
					ModelState.AddModelError("", "Invalid Request");
				}
			}

			//else
			return BadRequest(ModelState);

		}




		#endregion



		#region >>> ADMIN <<<

		[Authorize(Policy = Policies.Names.AllowAdmin)]
		[HttpGet("admin/users")]
		public async Task<IActionResult> AdminGetUsers()
		{
			var user = GetAppUser();
			if (user == null) { return new BadRequestObjectResult("Unable to get app user"); }

			//get AppUserScopes
			var userScopes = await _scopedRolesRepo.GetAllAsync(user.Scope);
			// ReSharper disable once PossibleMultipleEnumeration
			var userIds = userScopes.Select(m => m.UserId).ToList();


			//get users
			var users = await _userManager.Users
							.Where(m => userIds.Contains(m.Id))
							.OrderBy(m => m.Name)
							.ToListAsync();

			//update with scope, role & avatar from userScopes
			foreach (var item in users)
			{
				// ReSharper disable once PossibleMultipleEnumeration
				var usr = userScopes.FirstOrDefault(m => m.UserId == item.Id);
				if (usr != null)
				{
					item.Scope = usr.Scope;
					item.Role = usr.Role;
					item.Avatar = usr.Avatar;
				}
			}

			//done
			return new OkObjectResult(_mapper.Map<IEnumerable<Dtos.Identity.UserWithIdDto>>(users));
		}


		[Authorize(Policy = Policies.Names.AllowAdmin)]
		[HttpGet("admin/users/{id}")]
		public async Task<IActionResult> AdminGetUser([FromRoute] string id)
		{
			var who = GetAppUser();
			if (who == null) { return new BadRequestObjectResult("Unable to get app user"); }


			//get user
			var user = await _userManager.FindByIdAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			//get AppUserScope
			var userScope = await _scopedRolesRepo.FindAsync(id, who.Scope);
			if (userScope == null)
			{
				return NotFound();
			}

			//update with scope, role & avatar from userScopes
			user.Scope = userScope.Scope;
			user.Role = userScope.Role;
			user.Avatar = userScope.Avatar;

			//done
			return new OkObjectResult(_mapper.Map<Dtos.Identity.UserWithIdDto>(user));
		}



		[Authorize(Policy = Policies.Names.AllowAdmin)]
		[HttpPost("admin/users")]
		public async Task<IActionResult> AdminCreateUser([FromBody] Dtos.Identity.RegisterDto model, [FromQuery] string role = RegisteredRoles.viewer)
		{
			//initial validation
			if (ModelState.IsValid)
			{
				if (!RegisteredScopes.IsValid(model.Scope))
				{
					//verify scope
					ModelState.AddModelError("", "Invalid Request");
				}
				else if (!string.Equals(model.Hash, model.Scope))	//todo: make the create user hash be a bit more involved
				{
					//verify hash
					ModelState.AddModelError("", "Invalid Request");
				}
				else if (role != RegisteredRoles.viewer && role != RegisteredRoles.manager)
				{
					//invalid role
					ModelState.AddModelError("", "Invalid Request - unsupported role");
				}
				else
				{
					string avatar = new AvatarRepo().RandomAvatar();

					var user = await _userManager.FindByNameAsync(model.Email);
					if (user == null)
					{
						user = new AppUser()
						{
							Id = Guid.NewGuid().ToString("n"),
							UserName = model.Email.ToLower(),
							Email = model.Email.ToLower(),
							EmailConfirmed = true,
							PhoneNumber = model.Phone,
							PhoneNumberConfirmed = !string.IsNullOrEmpty(model.Phone),
							Name = model.Name,
							Role = role,
							Scope = model.Scope,
							Avatar = avatar
						};

						var result = await _userManager.CreateAsync(user, model.Password);
						if (!result.Succeeded)
						{
							return new BadRequestObjectResult(result.Errors.Select(m => m.Description));
						}
					}
					else if (!user.EmailConfirmed)
					{
						user.EmailConfirmed = true;
						var update = await _userManager.UpdateAsync(user);
						if (!update.Succeeded)
						{
							return new BadRequestObjectResult(update.Errors.Select(m => m.Description));
						}
					}

					//add user to scope
					if (await _scopedRolesRepo.FindAsync(user.Id, model.Scope) == null)
					{
						var scopedRole = await _scopedRolesRepo.CreateOrUpdateAsync(user.Id, model.Scope, role, avatar);
						user.Scope = scopedRole.Scope;
						user.Role = scopedRole.Role;
						user.Avatar = scopedRole.Avatar;
					}

					return new OkObjectResult(_mapper.Map<Dtos.Identity.UserWithIdDto>(user));

				}
			}

			//else
			return BadRequest(ModelState);
		}


		[Authorize(Policy = Policies.Names.AllowAdmin)]
		[HttpPatch("admin/users/{id}")]
		public async Task<IActionResult> AdminUpdateUser([FromRoute] string id, [FromBody] Dtos.PatchDto model)
		{
			//initial validation
			if (ModelState.IsValid)
			{
				var who = GetAppUser();
				if (who == null) { return new BadRequestObjectResult("Unable to get app user"); }

				var user = await _userManager.FindByIdAsync(id);
				if (user == null)
				{
					return NotFound();
				}

				var userScope = await _scopedRolesRepo.FindAsync(user.Id, who.Scope);
				if (userScope == null)
				{
					return NotFound(); //user is not part of this scope
				}

				try
				{
					user = await PatchUser(user, userScope, model);
				}
				catch (Repo.PatchException ex)
				{
					return new BadRequestObjectResult(new { reason = ex.Message });
				}

				//else
				return new OkObjectResult(_mapper.Map<Dtos.Identity.UserWithIdDto>(user));

			}

			//else
			return BadRequest(ModelState);

		}

		[Authorize(Policy = Policies.Names.AllowAdmin)]
		[HttpPatch("admin/users/{id}/multiple")]
		public async Task<IActionResult> AdminUpdateUser([FromRoute] string id, [FromBody] List<Dtos.PatchDto> model)
		{
			//initial validation
			if (ModelState.IsValid)
			{
				var who = GetAppUser();
				if (who == null) { return new BadRequestObjectResult("Unable to get app user"); }

				var user = await _userManager.FindByIdAsync(id);
				if (user == null)
				{
					return NotFound();
				}

				var userScope = await _scopedRolesRepo.FindAsync(user.Id, who.Scope);
				if (userScope == null)
				{
					return NotFound(); //user is not part of this scope
				}

				try
				{
					foreach (var patch in model)
					{
						user = await PatchUser(user, userScope, patch);
					}
				}
				catch (Repo.PatchException ex)
				{
					return new BadRequestObjectResult(new { reason = ex.Message });
				}

				//else
				return new OkObjectResult(_mapper.Map<Dtos.Identity.UserWithIdDto>(user));

			}

			//else
			return BadRequest(ModelState);

		}


		[Authorize(Policy = Policies.Names.AllowAdmin)]
		[HttpDelete("admin/users/{id}")]
		public async Task<IActionResult> AdminDeleteUser([FromRoute] string id, [FromQuery] string email)
		{
			var who = GetAppUser();
			if (who == null) { return new BadRequestObjectResult("Unable to get app user"); }


			//get user
			var user = await _userManager.FindByIdAsync(id);
			if (user == null || !string.Equals(user.Email, email, StringComparison.CurrentCultureIgnoreCase))
			{
				return NotFound();
			}

			//remove AppUserScope
			var success = await _scopedRolesRepo.DeleteAsync(id, who.Scope);
			if (!success)
			{
				return NotFound();
			}

			//is the user a member of any other scope
			var scopes = await _scopedRolesRepo.GetAllBelongingToAsync(id);
			if (scopes.Count() > 0)
			{
				return new OkObjectResult(new { success = true });

			} else
			{

				var result = await _userManager.DeleteAsync(user);

				if (result.Succeeded)
				{
					return new OkObjectResult(new { success = true });
				}
				//else
				return new BadRequestObjectResult(result.Errors);

			}


		}



		//-- SCOPE (requires ROOT user) --//

		[Authorize(Policy = Policies.Names.AllowRoot)]
		[HttpGet("admin/scopes/{id}")]
		public async Task<IActionResult> ScopeUsers([FromRoute] string id)
		{
			var who = GetAppUser();
			if (who == null) { return new BadRequestObjectResult("Unable to get app user"); }


			//get scope
			var scope = RegisteredScopes.Find(id);
			if (scope == null)
			{
				return new NotFoundObjectResult(new { reason = "invalid scope" });
			}

			var userScopes = await _scopedRolesRepo.GetAllAsync(scope.Id);
			// ReSharper disable once PossibleMultipleEnumeration
			var userIds = userScopes.Select(m => m.UserId).ToList();
			var users = await _userManager.Users.Where(m => userIds.Contains(m.Id)).ToListAsync();

			foreach (var user in users)
			{
				// ReSharper disable once PossibleMultipleEnumeration
				var userScope = userScopes.SingleOrDefault(m => m.UserId == user.Id);
				if (userScope != null)
				{
					//update user info
					user.Scope = userScope.Scope;
					user.Role = userScope.Role;
					user.Avatar = userScope.Avatar;
				}
			}
			return new OkObjectResult(_mapper.Map<IEnumerable<Dtos.Identity.UserWithIdDto>>(users));

		}


		//-- SEED SCOPE --//
		[Authorize(Policy = Policies.Names.AllowRoot)]
		[HttpPost("admin/scopes/{id}")]
		public async Task<IActionResult> SeedScopes([FromRoute] string id, [FromQuery] string email, [FromQuery] string role)
		{
			var who = GetAppUser();
			if (who == null) { return new BadRequestObjectResult("Unable to get app user"); }


			//get scope
			var scope = RegisteredScopes.Find(id);
			if (scope == null)
			{
				return new NotFoundObjectResult(new { reason = "invalid scope" });
			}

			//get user
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
			{
				return new NotFoundObjectResult(new { reason = "invalid user" });
			}

			//validate role
			role = role ?? RegisteredRoles.viewer;
			if (!RegisteredRoles.Names.Contains(role) || 
				(string.Equals(role, RegisteredRoles.root) && !(string.Equals(email, who.Email, StringComparison.CurrentCultureIgnoreCase))))
			{
				return new NotFoundObjectResult(new { reason = "invalid role" });
			} 

			//add the scope
			var userScope = await _scopedRolesRepo.CreateOrUpdateAsync(user.Id, scope.Id, role, null);
			return new OkObjectResult(userScope);

		}

		#endregion



		#region >> SEED <<


		[AllowAnonymous()]
		[HttpPost("admin/seed/users/{scope}/{hash}")]
		public async Task<IActionResult> SeedUsers([FromRoute] string scope, [FromRoute] string hash, List<SeededUser> model)
		{
			var calcHash = $"skippy-{scope.Replace("-", "").Substring(2, 8)}{DateTime.Now.Day}-alpha".ToBase64();
			if (!calcHash.Equals(hash))
			{
				return new UnauthorizedObjectResult(calcHash);
			}

			if (model.Count == 0)
			{
				return new BadRequestObjectResult("missing seeded users");
			}

			var created = new List<string>();
			var scoped = new List<string>();
			var errors = new Dictionary<string, string>();

			foreach (var seededUser in model)
			{
				if (SeededUsers.Contains(seededUser.Email))
				{
					var user = await _userManager.FindByEmailAsync(seededUser.Email);
					if (user == null)
					{
						try
						{
							//create the user
							user = new AppUser()
							{
								Id = Guid.NewGuid().ToString("n"),
								UserName = seededUser.Email.ToLowerInvariant(),
								Email = seededUser.Email,
								Name = seededUser.Name,
								EmailConfirmed = true
							};
							var result = await _userManager.CreateAsync(user, seededUser.Password);
							if (result.Succeeded)
							{
								created.Add(user.Email);
							}
							else
							{
								user = null;
								errors.Add($"create: {seededUser.Email}", ProcessIdentityErrors(result.Errors));
							}
						}
						catch (Exception e)
						{
							user = null;
							errors.Add($"create exception: {seededUser.Email}", $"creating user: {e.Message}");
							Console.WriteLine($"Error creating seeded user: {seededUser.Email}");
							Console.WriteLine(e.ToString());
							Console.WriteLine();
						}
						
					}

					if (user != null)
					{
						try
						{
							var userScoped = await _scopedRolesRepo.FindAsync(user.Id, scope);
							if (userScoped == null)
							{					
								string avatar = new AvatarRepo().RandomAvatar();
								await _scopedRolesRepo.CreateOrUpdateAsync(user.Id, scope, seededUser.Role, avatar);
								scoped.Add(seededUser.Email);
							}
						}
						catch (Exception e)
						{
							errors.Add($"scope exception: {seededUser.Email}", $"creating scope: {e.Message}");

						}
					}
				}
			}

			return new OkObjectResult(new {created, scoped, errors});
		}

		#region >> seeded users <<

		public class SeededUser
		{
			public string Email { get; set; }
			public string Name { get; set; }
			public string Password { get; set; }
			public string Role { get; set; }
		}
		private List<string> SeededUsers
		{
			get
			{
				return new List<string>()
				{
					"rob@myhallpass.com", "chester.b.tester@gmail.com", "nova.b.tester@gmail.com",
					"gail@hallpassandfriends.com", "scotia.b.tester@gmail.com"
				};
			}
		}

		private string IdentityErrorToString(IdentityError error)
		{
			if (error == null)
			{
				return "";
			}

			return $"{error.Code} - {error.Description}";
		}

		private string ProcessIdentityErrors(IEnumerable<IdentityError> errors)
		{
			return string.Join(" | ", errors.Select(IdentityErrorToString));
		}
		#endregion

		#endregion
		#region >> ... HELPERS ... <<

		private async Task<List<string>> ValidatePasswordStrengthAsync(AppUser user, string password)
		{
			var ret = new List<string>();

			foreach (var validator in _userManager.PasswordValidators)
			{
				var result = await validator.ValidateAsync(_userManager, user, password);
				if (!result.Succeeded)
				{
					if (!result.Errors.Any())
					{
						ret.Add("Invalid Password");
					} else
					{
						ret.AddRange(result.Errors.Select(m => m.Description));
					}
				}
			}

			return !ret.Any() ? null : ret;
		}

		private async Task<bool> ResetPasswordAsync(AppUser user, string scope, string password, string token)
		{
			if (string.IsNullOrEmpty(token))
			{
				throw new ArgumentNullException(nameof(token));
			}

			if (token.Length == _tokenRepo.KnownUserTokenLength(KnownUserToken.reset_password))
			{
				if (await _tokenRepo.ValidateAndDeleteAsync(user.Id, scope, KnownUserToken.reset_password, token))
				{
					return await ResetPasswordWithoutToken(user, scope, password);	//reset without token
				} else
				{
					return false;
				}
			}
			else
			{
				var result = await _userManager.ResetPasswordAsync(user, token, password);
				return result.Succeeded;
			}
		}

		private async Task<bool> ResetPasswordWithoutToken(AppUser user, string scope, string password)
		{
			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			return await ResetPasswordAsync(user, scope, password, token);
		}

		private async Task<Services.Email.EmailResponse> SendAccountActivationEmailAsync(AppUser user, string linkUrl = null)
		{
			if (user == null) { throw new ArgumentNullException(nameof(user)); }

			if (string.IsNullOrEmpty(linkUrl))
			{
				return await _SendAccountActivationCodeEmailAsync(user);
			}
			else
			{
				return await _SendAccountActivationLinkEmailAsync(user, linkUrl);
			}
		}
		private async Task<Services.Email.EmailResponse> _SendAccountActivationLinkEmailAsync(AppUser user, string linkUrl)
		{
			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
			var url = linkUrl.Replace("{email}", user.Email).Replace("{token}", token);
			var emailService = new Services.Email.EmailService(_config, user.Scope);
			return await emailService.SendEmailConfirmationLinkMessageAsync(new SendGrid.Helpers.Mail.EmailAddress(user.Email, user.Name), url);
		}

		private async Task<Services.Email.EmailResponse> _SendAccountActivationCodeEmailAsync(AppUser user)
		{
			var appToken = await _tokenRepo.GenerateTokenAsync(user.Id, user.Scope, KnownUserToken.confirm_email );
			if (appToken == null || appToken.IsExpired()) { throw new Exception("Error generating activation token"); }
			var expires = $"in {_tokenRepo.KnownUserTokenTTL(KnownUserToken.confirm_email)} minutes";
			var emailService = new Services.Email.EmailService(_config, user.Scope);
			return await emailService.SendEmailConfirmationCodeMessageAsync(new SendGrid.Helpers.Mail.EmailAddress(user.Email, user.Name), appToken.Token, expires);
		}

		private async Task<Services.Email.EmailResponse> SendPasswordResetEmailAsync(AppUser user, string linkUrl = null)
		{
			if (user == null) { throw new ArgumentNullException(nameof(user)); }

			if (string.IsNullOrEmpty(linkUrl))
			{
				return await _SendPasswordResetCodeEmailAsync(user);
			}
			else
			{
				return await _SendPasswordResetLinkEmailAsync(user, linkUrl);
			}
		}

		private async Task<Services.Email.EmailResponse> _SendPasswordResetLinkEmailAsync(AppUser user, string linkUrl)
		{
			var token = await _userManager.GeneratePasswordResetTokenAsync(user);
			var url = linkUrl.Replace("{email}", user.Email).Replace("{token}", token);
			var emailService = new Services.Email.EmailService(_config, user.Scope);
			return await emailService.SendPasswordResetLinkMessageAsync(new SendGrid.Helpers.Mail.EmailAddress(user.Email, user.Name), url);
		}

		private async Task<Services.Email.EmailResponse> _SendPasswordResetCodeEmailAsync(AppUser user)
		{
			var appToken = await _tokenRepo.GenerateTokenAsync(user.Id, user.Scope, KnownUserToken.reset_password );
			if (appToken == null || appToken.IsExpired()) { throw new Exception("Error generating password reset token"); }
			var expires = $"in {_tokenRepo.KnownUserTokenTTL(KnownUserToken.confirm_email)} minutes";
			var emailService = new Services.Email.EmailService(_config, user.Scope);
			return await emailService.SendEmailPasswordResetCodeMessageAsync(new SendGrid.Helpers.Mail.EmailAddress(user.Email, user.Name), appToken.Token, expires);
		}


		private string CalculateAuthHash(params string[] content)
		{
			var text = string.Join("", 
				content.Select(m => string.IsNullOrEmpty(m) ? null : string.Concat(m.First(), m.Last(), m.Length.ToString()))
				);
			return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(text.ToLower()));
		}

		[Authorize]
		[HttpGet("test")]
		public async Task<IActionResult> Test()
		{
			var who = GetAppUser();
			if (who != null)
			{
				var user = await _userManager.FindByEmailAsync(who.Email);
				if (user != null && string.Equals(who.SecurityStamp, user.SecurityStamp))
				{
					return new OkObjectResult(new { token = await _userManager.CreateSecurityTokenAsync(user) });
				}

			}

			return new BadRequestObjectResult("Token has expired");
		}



		private async Task<AppUser> PatchUser(AppUser user, AppUserScopedRole userScope, Dtos.PatchDto model)
		{
			Func<IEnumerable<IdentityError>, string> buildErrorMessage = (x) => string.Join(", ", x.Select(m => m.Description));


			switch (model.Op)
			{
				case Dtos.PatchOperation.replace:
					//used for changing email and phone
					string token;
					IdentityResult result;

					switch (model.Path.ToLower())
					{
						case "/name":
							user.Name = model.Value;
							result = await _userManager.UpdateAsync(user);
							if (!result.Succeeded)
							{
								throw new Repo.PatchException(buildErrorMessage(result.Errors));
							}
							break;

						case "/email":
							var email = model.Value.ToLower();
							token = await _userManager.GenerateChangeEmailTokenAsync(user, email);
							result = await _userManager.ChangeEmailAsync(user, email, token);
							if (result.Succeeded)
							{
								user.UserName = email;
								result = await _userManager.UpdateAsync(user);
							}
							if (result.Succeeded)
							{
								await _userManager.UpdateNormalizedUserNameAsync(user);
							}

							if (!result.Succeeded)
							{
								throw new Repo.PatchException(buildErrorMessage(result.Errors));
							}
							break;


						case "/email-confirmation":
						case "/emailconfirmation":
							user.EmailConfirmed = true;
							result = await _userManager.UpdateAsync(user);
							if (!result.Succeeded)
							{
								throw new Repo.PatchException(buildErrorMessage(result.Errors));
							}
							break;


						case "/password":
							var errors = await ValidatePasswordStrengthAsync(user, model.Value);
							if (errors != null)
							{
								throw new Repo.PatchException(string.Join(", ", errors));
							}

							var success = await ResetPasswordWithoutToken(user, userScope.Scope, model.Value);

							if (!success)
							{
								throw new Repo.PatchException("Unable to save password");
							}
							break;


						case "/role":
							if (!RegisteredRoles.Names.Contains(model.Value))
							{
								throw new Repo.PatchException("Invalid role");
							}
							userScope = await _scopedRolesRepo.UpdateRoleAsync(user.Id, userScope.Scope, model.Value);

							break;

						case "/phonenumber":
						case "/phone":
							token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.Value);
							result = await _userManager.ChangePhoneNumberAsync(user, model.Value, token);
							if (!result.Succeeded)
							{
								throw new Repo.PatchException(buildErrorMessage(result.Errors));
							}
							break;

						case "/avatar":
							if (string.IsNullOrEmpty(model.Value))
							{
								model.Value = new AvatarRepo().RandomAvatar();
							}
							var scopedUserForAvatar = await _scopedRolesRepo.UpdateAvatarAsync(user.Id, userScope.Scope, model.Value);
							user.Avatar = scopedUserForAvatar.Avatar;
							break;

						case "/trelloid":
							var scopedUserForTrelloId = await _scopedRolesRepo.UpdateTrelloIdAsync(user.Id, userScope.Scope, model.Value);
							user.TrelloId = scopedUserForTrelloId.TrelloId;
							break;

						default:
							throw new Repo.PatchException("Unsupported patch path: " + model.Path);

					}

					//fill in the blanks
					user.Scope = userScope.Scope;
					user.Role = userScope.Role;
					//only add avatar if it was not changed
					if (string.IsNullOrEmpty(user.Avatar))
					{
						user.Avatar = userScope.Avatar;
					}

					break;

				//case Dtos.PatchOperation.add:
				//case Dtos.PatchOperation.remove:
				//case Dtos.PatchOperation.delete:
				default:
					throw new Repo.PatchException("Unsupported patch operation: " + model.Op.ToString());
			}

			//done
			return user;
		}
		#endregion




		#region >>--- Helper Return Classes ---<<


		protected class AuthRegistrationOk
		{
			public Dtos.Identity.UserDto User { get; set; }
			public bool Activated { get; set; }
			public Services.Email.EmailResponse Response { get; set; }

			public AuthRegistrationOk()
			{ }

			public AuthRegistrationOk(Dtos.Identity.UserDto user, bool activated, Services.Email.EmailResponse response = null)
			{
				User = user;
				Activated = activated;
				Response = response;
			}
		}

		protected class AuthRegistrationDuplicate
		{
			public bool Duplicate { get; set; }
			public bool Activated { get; set; }
			public Services.Email.EmailResponse Response { get; set; }

			public AuthRegistrationDuplicate(bool activated = false, Services.Email.EmailResponse response = null)
			{
				Duplicate = true;
				Activated = activated;
				Response = response;
			}

		}

		public class AuthLoginFailed
		{
			public enum FailReason
			{
				invalid,
				lockout,
				// ReSharper disable once InconsistentNaming
				not_activated
			}
			public FailReason Reason { get; private set; }
			public string Message { get; private set; }

			public AuthLoginFailed(FailReason reason)
			{
				Reason = reason;
				switch (reason)
				{
					case FailReason.invalid:
						Message = "Invalid email and/or password";
						break;
					case FailReason.lockout:
						Message = "Your account has been locked due to too many invalid login attempts.  It will be reset in 10 minutes.";
						break;
					case FailReason.not_activated:
						Message = "Your email has not been confirmed";
						break;
					// ReSharper disable once RedundantEmptySwitchSection
					default:
						break;
				}
			}
		}
		#endregion
	}
}