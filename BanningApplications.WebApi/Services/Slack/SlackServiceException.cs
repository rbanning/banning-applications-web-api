using System;
using System.Collections.Generic;
using System.Net.Http;

namespace BanningApplications.WebApi.Services.Slack
{
    public class SlackServiceException: Exception
    {
		public HttpResponseMessage Response { get; set; }

		public List<string> Details { get; set; }

		public SlackServiceException(string message)
			: base(message)
		{
			ProcessDetails();
		}
		public SlackServiceException(string message, List<string> details)
			: base(message)
		{
			ProcessDetails(null, details);
		}

		public SlackServiceException(string message, Exception inner)
			: base(message, inner)
		{
			ProcessDetails();
		}

		public SlackServiceException(string message, SlackError error)
			: base(message)
		{
			ProcessDetails(error);
		}

		public SlackServiceException(string message, Exception inner, List<string> details)
			: base(message, inner)
		{
			ProcessDetails(null, details);
		}

		public SlackServiceException(string message, SlackError error, List<string> details)
			: base(message)
		{
			ProcessDetails(error, details);
		}


		protected void ProcessDetails(SlackError err = null, List<string> details = null)
		{
			Details = new List<string>();

			var ex = this as Exception;
			while (ex != null)
			{
				Details.Add(ex.Message);
				ex = ex.InnerException;
			}

			if (err != null)
			{
				Details.Add(err.Message);
			}

			if (details?.Count > 0)
			{
				Details.AddRange(details);
			}
		}
	}
}
