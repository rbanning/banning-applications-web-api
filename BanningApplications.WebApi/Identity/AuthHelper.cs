using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Identity
{
	public static class AuthHelper
	{

		#region >> METHODS <<


		public static void ConfigureAuthorization(IServiceCollection services, IConfiguration config)
		{

			services.AddAuthorization(options =>
			{

				//role based policies
				options.AddPolicy(Policies.Names.AllowRoot,
					policy => policy
						.Requirements
						.Add(new AuthReq.RoleRequirement(RegisteredRoles.root)));

				options.AddPolicy(Policies.Names.AllowAdmin,
					policy => policy
						.Requirements
						.Add(new AuthReq.RoleRequirement(
							RegisteredRoles.root, RegisteredRoles.admin)));

				options.AddPolicy(Policies.Names.AllowManager,
					policy => policy
						.Requirements
						.Add(new AuthReq.RoleRequirement(
							RegisteredRoles.root, RegisteredRoles.admin, RegisteredRoles.manager)));

				options.AddPolicy(Policies.Names.AllowCustomer,
					policy => policy
						.Requirements
						.Add(new AuthReq.RoleRequirement(
							RegisteredRoles.root, RegisteredRoles.admin, RegisteredRoles.manager, RegisteredRoles.customer)));

				options.AddPolicy(Policies.Names.AllowViewer,
					policy => policy
						.Requirements
						.Add(new AuthReq.RoleRequirement(
							RegisteredRoles.root, RegisteredRoles.admin, RegisteredRoles.manager, RegisteredRoles.customer, RegisteredRoles.viewer)));

				//scope based polices
				foreach (var scope in RegisteredScopes.All())
				{
					options.AddPolicy($"scope:{scope.Code}",
						policy => policy
							.Requirements
							.Add(new AuthReq.ScopeRequirement(scope.Id)));

				}

			});

			//register the requirement handlers
			services.AddSingleton<IAuthorizationHandler, AuthReq.RoleReqHandler>();
			services.AddSingleton<IAuthorizationHandler, AuthReq.ScopeReqHandler>();
		}

		#endregion

	}

}
