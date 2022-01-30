using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BanningApplications.WebApi.Helpers.trg
{
    public static class TestingHelper
    {
		public static string TemplatePath(bool asFullPath, string filename = null)
		{
			var parts = new List<string>
			{
				asFullPath ? Directory.GetCurrentDirectory() : null,
				"uploads", "trg", "test-form-templates",
				filename
			};

			return Path.Combine(parts.Where(m => !string.IsNullOrEmpty(m)).ToArray());
		}

		//assert: need to have a unique path for report PDF's.  
		//		the path should be a combination of...
		//		associated test id 
		//		the template id
		public static string TestReportPath(string testId, string templateId, string extension)
		{
			if (testId == null)
			{
				throw new ArgumentNullException(nameof(testId));
			}
			if (templateId == null)
			{
				throw new ArgumentNullException(nameof(templateId));
			}
			if (extension == null)
			{
				throw new ArgumentNullException(nameof(extension));
			}



			if (!extension.StartsWith(('.')))
			{
				extension = "." + extension;
			}

			return Path.Combine(TestReportFolder(testId), templateId.ConvertToBlobSegment() + extension);
		}

		public static string TestReportFolder(string testId)
		{
			if (testId == null)
			{
				throw new ArgumentNullException(nameof(testId));
			}

			return testId.ConvertToBlobSegment();
		}

	}
}
