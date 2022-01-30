using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BanningApplications.WebApi.Services.File
{
    public class FileService: IFileService
    {

		#region >> STATIC <<

		// ReSharper disable once InconsistentNaming
		private static readonly Dictionary<string, string> _contentTypeDict = new Dictionary<string, string>()
		{
			{ "image/png", ".png" },
			{ "image/jpeg", ".jpg" },
			{ "image/svg+xml", ".svg" },
			{ "text/plain", ".txt" },
			{ "application/json", ".json" },
			{ "application/pdf", ".pdf" },
		};

		public static readonly List<string> AcceptedContentTypes = _contentTypeDict.Keys.ToList();
		public static readonly List<string> AcceptedExtensions = _contentTypeDict.Values.ToList();


		public static string ExtensionToContentType(string extension)
		{
			if (!extension.StartsWith(".")) { extension = "." + extension; }
			return _contentTypeDict.FirstOrDefault(m => m.Value == extension.ToLower()).Key;
		}

		public static string ContentTypeToExtension(string contentType)
		{
			return _contentTypeDict.FirstOrDefault(m => m.Key == contentType.ToLower()).Value;
		}


		public static string SanitizeFilename(string filename, string replaceInvalidCharsWith = "", string replaceSpaceWith = "-")
	    {
			//does not look for reserved words
			//https://en.wikipedia.org/wiki/Filename#Reserved_characters_and_words

			var whitespaceRx = @"\s+";
			filename = Regex.Replace(
				filename,
				whitespaceRx,
				replaceSpaceWith);
			
			var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
		    var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
		    
		    return Regex.Replace(
			    filename,
			    invalidRegStr, replaceInvalidCharsWith);
		}


		#endregion


	    #region >> IFileService <<

	    public async Task<bool> SaveTextToFileAsync(string text, string pathAndFilename, bool overwrite = false)
	    {
		    if (string.IsNullOrEmpty(text)) { throw new ArgumentNullException(nameof(text)); }

		    //prepare by ensuring that the folder exists
		    EnsureDirectoryExists(Path.GetDirectoryName(pathAndFilename));

		    //prepare by being sure there is no file at pathAndFilename
		    if (overwrite)
		    {
			    DeleteFile(pathAndFilename);
		    }
		    else
		    {
			    VersionFile(pathAndFilename);   //create version of the file if it exists
		    }

		    await System.IO.File.WriteAllTextAsync(pathAndFilename, text);
		    return System.IO.File.Exists(pathAndFilename);
	    }


		public async Task<bool> SaveBase64ToFileAsync(string data, string pathAndFilename, bool overwrite = false)
		{
			if (string.IsNullOrEmpty(data)) { throw new ArgumentNullException(nameof(data)); }

			//prepare by ensuring that the folder exists
			EnsureDirectoryExists(Path.GetDirectoryName(pathAndFilename));

			//prepare by being sure there is no file at pathAndFilename
			if (overwrite)
			{
				DeleteFile(pathAndFilename);
			}
			else
			{
				VersionFile(pathAndFilename);   //create version of the file if it exists
			}

			var bytes = Convert.FromBase64String(data);
			await System.IO.File.WriteAllBytesAsync(pathAndFilename, bytes);
			return System.IO.File.Exists(pathAndFilename);
		}
		public async Task<bool> SaveFormFileAsync(IFormFile file, string pathAndFilename, bool overwrite = false)
		{
			if (file == null || file.Length == 0) { throw new ArgumentNullException(nameof(file)); }

			//prepare by ensuring that the folder exists
			EnsureDirectoryExists(Path.GetDirectoryName(pathAndFilename));

			//prepare by being sure there is no file at pathAndFilename
			if (overwrite)
			{
				DeleteFile(pathAndFilename);
			}
			else
			{
				VersionFile(pathAndFilename);   //create version of the file if it exists
			}

			await using (Stream fs = new FileStream(pathAndFilename, FileMode.Create))
			{
				await file.CopyToAsync(fs);
			}
			return System.IO.File.Exists(pathAndFilename);
		}

		public void VersionFile(string pathAndFilename)
		{
			var ext = Path.GetExtension(pathAndFilename);
			int version = 0;
			var targetName = pathAndFilename;

			while (FileExists(targetName))
			{
				version += 1;
				targetName = string.Concat(
				pathAndFilename.Substring(0, pathAndFilename.Length - ext.Length),
				$"--{version}",
				ext);
			}

			if (version > 0)
			{
				System.IO.File.Move(pathAndFilename, targetName);
			}
		}

		public void RenameFile(string currentPathAndFilename, string newPathAndFilename, bool overwrite = false)
		{
			if (System.IO.File.Exists(currentPathAndFilename))
			{
				//prepare by being sure there is no file at newPathAndFilename
				if (overwrite)
				{
					DeleteFile(newPathAndFilename);
				}
				else
				{
					VersionFile(newPathAndFilename);   //create version of the file if it exists
				}

				System.IO.File.Move(currentPathAndFilename, newPathAndFilename);
			}
		}

		public void DeleteFile(string pathAndFilename)
		{
			if (FileExists(pathAndFilename))
			{
				System.IO.File.Delete(pathAndFilename);
			}
		}

		public bool FileExists(string pathAndFilename)
		{
			return System.IO.File.Exists(pathAndFilename);
		}

		public void EnsureDirectoryExists(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}


		#endregion

	}
}
