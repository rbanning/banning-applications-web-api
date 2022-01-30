using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace BanningApplications.WebApi.Helpers
{
    public class ImageProcessor
    {
		public const string FOLDER = "WorkingPhotos";

		private IWebHostEnvironment _hostingEnvironment;
		private List<string> _imageContentTypes;


		public ImageProcessor(IWebHostEnvironment hostingEnvironment)
			:this(hostingEnvironment, DefaultImageContentTypes())
		{ }

		public ImageProcessor(IWebHostEnvironment hostingEnvironment, List<string> contentTypes)
		{
			_hostingEnvironment = hostingEnvironment;
			_imageContentTypes = contentTypes ?? DefaultImageContentTypes();
		}

		public string FileLocation(string folderName, string baseFileName)
		{
			return Path.Combine(_hostingEnvironment.WebRootPath, FOLDER, folderName, baseFileName);
		}

		public string NormalizeContentType(string contentType)
		{
			//special case
			if (string.Equals(contentType, "jpg", StringComparison.CurrentCultureIgnoreCase))
			{
				return NormalizeContentType("image/jpeg");
			}

			var types = DefaultImageContentTypes();
			var ret = types.FirstOrDefault(m => string.Equals(m, contentType, StringComparison.CurrentCultureIgnoreCase));
			if (string.IsNullOrEmpty(ret))
			{
				contentType = contentType.ToLower();
				ret = types.FirstOrDefault(m => m.ToLower().EndsWith(contentType));
			}
			return ret;
		}

		public string UploadImage(IFormFile file, string folderName)
		{
			if (file == null) { throw new ArgumentNullException("file"); }

			if (!_imageContentTypes.Contains(file.ContentType)) { throw new ArgumentException("Unsupported image file content type"); }
			if (file.Length <= 0) { throw new ArgumentException("File does not contain any content/data"); }

			//directory
			string fileName = System.Net.Http.Headers.ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
			folderName = FileLocation(folderName, fileName);
			if (!Directory.Exists(folderName))
			{
				Directory.CreateDirectory(folderName);
			}
			string fullPath = Path.Combine(folderName, ImageInfo.FileName.Original(file.ContentType));
			using (var stream = new FileStream(fullPath, FileMode.Create))
			{
				file.CopyTo(stream);
			}

			//thumbs
			ResizeImage(fullPath, Path.Combine(folderName, ImageInfo.FileName.Thumb(file.ContentType, ImageInfo.THUMB_SMALL)), ImageInfo.THUMB_SMALL);
			ResizeImage(fullPath, Path.Combine(folderName, ImageInfo.FileName.Thumb(file.ContentType, ImageInfo.THUMB_MED)), ImageInfo.THUMB_MED);
			ResizeImage(fullPath, Path.Combine(folderName, ImageInfo.FileName.Thumb(file.ContentType, ImageInfo.THUMB_LG)), ImageInfo.THUMB_LG);
			ResizeImage(fullPath, Path.Combine(folderName, ImageInfo.FileName.Thumb(file.ContentType, ImageInfo.THUMB_XL)), ImageInfo.THUMB_XL);

			return fullPath;
		}

		public ImageInfo ResizeImage(string filepath, string resizedpath, int width, int height)
		{
			//TODO: Should we change this to use generic Image (https://stackoverflow.com/a/57794299)
			using (Image<Rgba32> image = Image.Load<Rgba32>(filepath))
			{
				image.Mutate(x => x.Resize(width, height));
				image.Save(resizedpath);

				return new ImageInfo()
				{
					Path = resizedpath,
					Width = width,
					Height = height
				};
			}
		}

		public ImageInfo ResizeImage(string filepath, string resizedpath, int width)
		{
			//TODO: Should we change this to use generic Image (https://stackoverflow.com/a/57794299)
			using Image<Rgba32> image = Image.Load<Rgba32>(filepath);
			decimal ratio = width / image.Width;
			int height = (int)Math.Floor(image.Height * ratio);
			image.Mutate(x => x.Resize(width, height));
			image.Save(resizedpath);

			return new ImageInfo()
			{
				Path = resizedpath,
				Width = width,
				Height = height
			};
		}

		public bool ArchiveImageSet(string fileLocation)
		{
			if (Directory.Exists(fileLocation))
			{
				string newLocation = fileLocation;
				if (newLocation.EndsWith(Path.DirectorySeparatorChar)) { newLocation = newLocation.Substring(0, newLocation.Length - 1); }
				newLocation += "_ARCHIVED";
				try
				{
					Directory.Move(fileLocation, newLocation);
					return true;
				}
				catch (Exception)
				{
					return false;
				}
			}

			// else
			return false;
		}

		public static List<string> DefaultImageContentTypes()
		{
			return new List<string>()
			{
				"image/jpeg",	//JPEG
				"image/pjpeg",	//JPEG (progressive)
				"image/png"		//PNG
			};
		}
    }
}
