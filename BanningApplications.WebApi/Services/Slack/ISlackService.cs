using System.Threading.Tasks;
using BanningApplications.WebApi.Identity;

namespace BanningApplications.WebApi.Services.Slack
{
    public interface ISlackService
    {
		bool IsConfigured { get; }
	    SlackConfig Config { get; }

	    void Configure(SlackConfig config);
	    void Configure(RegisteredScopes.Scope scope);

		void Configure(string scope /* can be scope id or scope code */);


		Task<string> SendMessageAsync(string channel, string message);
    }
}
