using System.Threading.Tasks;
using BanningApplications.WebApi.Data;
using BanningApplications.WebApi.Dtos;
using BanningApplications.WebApi.Entities.unsplash;
using Microsoft.EntityFrameworkCore;

namespace BanningApplications.WebApi.Repo.unsplash
{
	public interface IUnsplashPhotographersRepository : IGenericBaseRepository<UnsplashPhotographer>
	{
        //getters
        Task<UnsplashPhotographer> GetByUsernameAsync(string username, bool asTracked = false);

        //exists
        Task<bool> ExistsByUsernameAsync(string username);

        //setters
	}

	public class UnsplashPhotographersRepository: GenericBaseRepository<UnsplashPhotographer, UnsplashDbContext>, IUnsplashPhotographersRepository
    {

		public UnsplashPhotographersRepository(UnsplashDbContext context)
			:base(context)
	    {}


		#region >> GETTERS <<

		public async Task<UnsplashPhotographer> GetByUsernameAsync(string username, bool asTracked = false)
		{
			return await Query(true, !asTracked).FirstOrDefaultAsync(m => m.UserName == username);
		}

		#endregion

		#region >> Exists <<

		public async Task<bool> ExistsByUsernameAsync(string username)
		{
			return await _context.UnsplashPhotographers.AnyAsync(m => m.UserName == username);
		}

		#endregion

		#region >> PATCHES <<

		protected override UnsplashPhotographer PatchReplace(UnsplashPhotographer model, PatchDto patch, string modifiedBy)
		{
			var processed = false;

			switch (patch.Path.ToLowerInvariant())
			{
				case "/username":
					model.UserName = patch.Value;
					processed = true;
					break;
				case "/name":
					model.Name = patch.Value;
					processed = true;
					break;
				case "/location":
					model.Location = patch.Value;
					processed = true;
					break;
				case "/bio":
					model.Bio = patch.Value;
					processed = true;
					break;
				case "/portfolio":
					model.Portfolio = patch.Value;
					processed = true;
					break;
			}

			if (processed)
			{
				return UpdateEntityMeta(model, modifiedBy);
			}
			//else
			return model;
		}

		#endregion
    }
}
