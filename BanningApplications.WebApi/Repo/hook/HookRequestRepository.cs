using BanningApplications.WebApi.Data;
using BanningApplications.WebApi.Entities.hook;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Repo.hook
{
	public interface IHookRequestRepository: IDataRepository
	{
		Task<HookRequest> Get(string id);
		Task<HookRequest> GetLast();
		Task<IEnumerable<HookRequest>> GetAll();
		Task<IEnumerable<HookRequest>> GetAll(int count);
		Task<IEnumerable<HookRequest>> GetAllForPath(string path);

		Task<HookRequest> Create(HookRequest model);

	}

	public class HookRequestRepository : IHookRequestRepository
	{
		private HookDbContext _context;

		public HookRequestRepository(HookDbContext context)
		{
			_context = context;
		}


		public async Task<HookRequest> Get(string id)
		{
			return await _context.HookRequests.FindAsync(id);
		}

		public async Task<HookRequest> GetLast()
		{
			return await _context.HookRequests
					.OrderByDescending(m => m.CreatedDate)
					.Take(1)
					.SingleOrDefaultAsync();
		}

		public async Task<IEnumerable<HookRequest>> GetAll()
		{
			return await _context.HookRequests
					.OrderByDescending(m => m.CreatedDate)
					.ToListAsync();
		}

		public async Task<IEnumerable<HookRequest>> GetAll(int count)
		{
			return await _context.HookRequests
					.OrderByDescending(m => m.CreatedDate)
					.Take(count)
					.ToListAsync();
		}

		public async Task<IEnumerable<HookRequest>> GetAllForPath(string path)
		{
			if (string.IsNullOrEmpty(path)) { throw new ArgumentNullException("path"); }
			path = path.ToLower();
			return await _context.HookRequests
				.Where(m => m.Path == path)
				.OrderByDescending(m => m.CreatedDate)
				.ToListAsync();
		}

		public async Task<HookRequest> Create(HookRequest model)
		{
			if (model == null) { throw new ArgumentNullException("model"); }

			if (string.IsNullOrEmpty(model.Id))
			{
				model.Id = Guid.NewGuid().ToString("n");
			}
			model.CreatedDate = DateTime.UtcNow;

			await _context.HookRequests.AddAsync(model);
			return model;
		}

		public async Task<int> SaveAsync()
		{
			return await _context.SaveChangesAsync();
		}
	}
}
