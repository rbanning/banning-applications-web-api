using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Services
{
    public static class Known
    {
		public const string QUEUE_CONTACT = "contact";
		public const string QUEUE_NOTIFICATION = "notification";

		public const string CONTAINER_KEY_TRG_TEST_FORMS = "trg-test-forms";
		public const string CONTAINER_KEY_TRG_TEST_TEMPLATES = "trg-test-templates";

		public static Dictionary<string, string> Queues { get; } = new Dictionary<string, string>()
		{
			{ "Contact", QUEUE_CONTACT },
			{ "Notification", QUEUE_NOTIFICATION }
		};

	}
}
