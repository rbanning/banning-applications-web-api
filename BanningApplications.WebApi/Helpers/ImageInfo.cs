using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Helpers
{
    public class ImageInfo
    {
		public const int THUMB_SMALL = 100;
		public const int THUMB_MED = 250;
		public const int THUMB_LG = 500;
		public const int THUMB_XL = 1200;

		public string Path { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		public static class FileName
		{
			public static string FilenameFromContentType(string name, string contentType)
			{
				name = name.ToLower();
				var ext = contentType.Split('/').Last().ToLower();
				if (ext.Contains("jpeg")) { ext = "jpg"; }
				return $"{name}.{ext}";
			}

			public static string Original(string contentType) {
				return FilenameFromContentType("orginal", contentType);
			}
			public static string Thumb(string contentType, int size) {
				return FilenameFromContentType($"thumb{size}", contentType);
			}

		}

		public static int ParseSize(string size)
		{
			if (!string.IsNullOrEmpty(size))
			{
				switch (size.ToUpper())
				{
					case "THUMB_SMALL":
					case "THUMB_SM":
					case "SMALL":
					case "SM":
						return THUMB_SMALL;

					case "THUMB_MEDIUM":
					case "THUMB_MED":
					case "MEDIUM":
					case "MED":
						return THUMB_MED;

					case "THUMB_LARGE":
					case "THUMB_LG":
					case "LARGE":
					case "LG":
						return THUMB_LG;

					case "THUMB_X-LARGE":
					case "THUMB_X_LARGE":
					case "THUMB_XL":
					case "X-LARGE":
					case "X_LARGE":
					case "XL":
						return THUMB_XL;


					default:
						break;
				}

			}

			//else
			return -1;
		}

	}
}
