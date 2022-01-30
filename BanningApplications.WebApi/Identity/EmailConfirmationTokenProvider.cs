using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace BanningApplications.WebApi.Identity
{
	public class EmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
	{
		public EmailConfirmationTokenProvider(
			IDataProtectionProvider dataProtectionProvider, 
			IOptions<EmailConfirmationTokenProviderOptions> options,
			ILogger<EmailConfirmationTokenProvider<TUser>> logger)
			: base(dataProtectionProvider, options, logger)
		{ }
	}

	public class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
	{

	}
}
