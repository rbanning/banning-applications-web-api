using System.Collections.Generic;

namespace BanningApplications.WebApi.Services.Slack
{
    public class SlackConfig
    {
	    public string Workspace { get; set; }
	    public List<SlackWebHook> WebHooks { get; set; }

	    public bool IsValid =>
		    !string.IsNullOrEmpty(Workspace)
		    && WebHooks.Count > 0;

	    public SlackConfig()
	    {
		    WebHooks = new List<SlackWebHook>();
	    }

	    public SlackConfig(string workspace)
			:this()
	    {
		    Workspace = workspace;
	    }


	    public static SlackConfig Hallpass()
	    {
		    var config = new SlackConfig("hallpassandfriends.slack.com");
			config.WebHooks.Add(new SlackWebHook("hallpass-dev", "https://hooks.slack.com/services/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"));

			return config;
	    }

	    public static SlackConfig Trg()
	    {
		    var config = new SlackConfig("trgfs.slack.com");
			//todo: replace the webhooks with real trgfs.slack.com urls
			config.WebHooks.Add(new SlackWebHook("hallpass-dev", "https://hooks.slack.com/services/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"));

			return config;
	    }



	    public class SlackWebHook
	    {
		    public string Channel { get; set; }
		    public string Url { get; set; }

		    public SlackWebHook()
		    { }

		    public SlackWebHook(string channel, string url)
				:this()
		    {
			    Channel = channel;
			    Url = url;
		    }

	    }
    }


}
