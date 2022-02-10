using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BanningApplications.WebApi.Data;
using BanningApplications.WebApi.Entities.unsplash;

namespace BanningApplications.WebApi.Repo.unsplash
{
	public interface IGameScoresRepository : IGenericBaseRepository<GameScore>
	{
		Task<IList<GameScore>> GetByEmailAsync(string email, string game = null, bool inclArchived = false);
		Task<IList<GameScore>> GetByGameAsync(string game, bool inclArchived = false);
	}
    public class GameScoresRepository: GenericBaseRepository<GameScore, UnsplashDbContext>, IGameScoresRepository
    {
	    public GameScoresRepository(UnsplashDbContext context)
		    : base(context)
	    { }


	    #region >>> Additional Getters <<<

	    public async Task<IList<GameScore>> GetByEmailAsync(string email, string game = null, bool inclArchived = false)
	    {
		    if (game == null)
		    {
			    return await GetAllAsync(m => m.Email == email, inclArchived);
		    }
		    //else
		    return await GetAllAsync(m => m.Email == email && m.Game == game, inclArchived);

	    }

	    public async Task<IList<GameScore>> GetByGameAsync(string game, bool inclArchived = false)
	    {
		    return await GetAllAsync(m => m.Game == game, inclArchived);
	    }


	    #endregion
    }
}
