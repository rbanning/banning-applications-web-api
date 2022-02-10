using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanningApplications.WebApi.Data;
using BanningApplications.WebApi.Dtos;
using BanningApplications.WebApi.Entities.unsplash;
using Microsoft.EntityFrameworkCore;

namespace BanningApplications.WebApi.Repo.unsplash
{
	public interface IUnsplashAwardWinnersRepository : IGenericBaseRepository<UnsplashAwardWinner>
	{
        //Additional Getters
        Task<IList<UnsplashAwardWinner>> GetAllByCategory(string category, int? year = null, bool inclArchived = false);
        Task<IList<UnsplashAwardWinner>> GetAllWinners(int? year = null, bool inclArchived = false);
        Task<Dictionary<string, int>> GetCategories(int year, bool inclArchived = false);

        //Additional Exists
        Task<bool> PhotoExistsAsync(string photoId);
        Task<bool> AwardWinnerExistsAsync(string photoId, string category, int year);
	}
    public class UnsplashAwardWinnersRepository: GenericBaseRepository<UnsplashAwardWinner, UnsplashDbContext>, IUnsplashAwardWinnersRepository
    {
	    public UnsplashAwardWinnersRepository(UnsplashDbContext context)
			:base(context)
	    { }

	    #region >> ADDITIONAL GETTERS <<

	    public async Task<IList<UnsplashAwardWinner>> GetAllByCategory(string category, int? year = null,
		    bool inclArchived = false)
	    {
		    return await GetAllAsync(m => m.Category == category && (!year.HasValue || m.Year == year.Value),
			    inclArchived);
	    }

	    public async Task<IList<UnsplashAwardWinner>> GetAllWinners(int? year = null, bool inclArchived = false)
	    {
		    return await GetAllAsync(m => m.Winner == true && (!year.HasValue || m.Year == year.Value),
			    inclArchived);
	    }

	    public async Task<Dictionary<string, int>> GetCategories(int year, bool inclArchived = false)
	    {
		    var query = _context.UnsplashAwardWinners
			    .AsNoTracking()
			    .Where(m => m.Year == year);
		    if (!inclArchived)
		    {
			    query = query.Where(m => m.Archived == false);
		    }

		    var results = await query
			    .GroupBy(m => m.Category)
			    .Select(m => new { category = m.Key, count = m.Count() })
			    .ToDictionaryAsync(m => m.category, m => m.count);

		    return results;
	    }


	    //Additional Exists
	    public async Task<bool> PhotoExistsAsync(string photoId)
	    {
		    return await _context.UnsplashPhotos.AnyAsync(m => m.Id == photoId);
	    }

	    public async Task<bool> AwardWinnerExistsAsync(string photoId, string category, int year)
	    {
		    return await _context.UnsplashAwardWinners.AnyAsync(m =>
			    m.PhotoId == photoId && m.Category == category && m.Year == year);
	    }


	    #endregion

	    #region >> OVERRIDE QUERY <<

	    protected override IQueryable<UnsplashAwardWinner> Query(bool inclArchived, bool asNoTracking = true)
	    {
		    if (asNoTracking)
		    {
			    // ReSharper disable once RedundantArgumentDefaultValue
			    return base.Query(inclArchived, true)
				    .Include(m => m.Photo);
		    }
			//else if there is tracking (asNoTracking == false)
			//return without any includes
		    return base.Query(inclArchived, false);
	    }


	    #endregion


	    #region >> PATCHES <<

	    protected override UnsplashAwardWinner PatchReplace(UnsplashAwardWinner model, PatchDto patch, string modifiedBy)
	    {
		    var processed = false;
		    switch (patch.Path.ToLowerInvariant())
		    {
			    case "/photo":
			    case "/photoid":
				    model.PhotoId = patch.Value;
				    processed = true;
				    break;
			    case "/category":
				    model.Category = patch.Value;
				    processed = true;
				    break;

				//int
				case "/year":
					if (int.TryParse(patch.Value, out int year))
					{
						model.Year = year;
						processed = true;
						break;
					}
					else
					{
						throw new PatchException("Invalid value for the year - expected an integer");
					}

				//bool
				case "/winner":
					if (bool.TryParse(patch.Value, out bool winner))
					{
						model.Winner = winner;
						processed = true;
						break;
					}
					else
					{
						throw new PatchException("Invalid value for winner - expected a boolean");
					}
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
