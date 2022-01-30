using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BanningApplications.WebApi.Services.File
{
    public interface IFileService
    {

	    Task<bool> SaveTextToFileAsync(string text, string pathAndFilename, bool overwrite = false);

		Task<bool> SaveBase64ToFileAsync(string data, string pathAndFilename, bool overwrite = false);
		Task<bool> SaveFormFileAsync(IFormFile file, string pathAndFilename, bool overwrite = false);

		void VersionFile(string pathAndFilename);
		void DeleteFile(string pathAndFilename);
		void RenameFile(string currentPathAndFilename, string newPathAndFilename, bool overwrite = false);
		bool FileExists(string pathAndFilename);
		void EnsureDirectoryExists(string path);
	}
}
