using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BanningApplications.WebApi.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BanningApplications.WebApi.Repo
{
	public interface IGenericBaseRepository<TEntity>: IDataRepository where TEntity: Entities.BaseMetaExtendedEntity
	{
		#region GETTERS

		Task<IList<TEntity>> GetAllAsync(bool inclArchived = false);
		Task<IList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter, bool inclArchived = false);
		Task<TEntity> GetAsync(string id, bool asTracked = false);
		Task<TEntity> FindFirstAsync(Expression<Func<TEntity, bool>> filter, bool inclArchived = false, bool asTracked = false);

		#endregion

		#region EXISTS

		Task<bool> ExistsAsync(string id);

		#endregion

		#region SETTERS

		Task<TEntity> CreateAsync(TEntity model, string modifiedBy);

		TEntity Patch(TEntity model, PatchDto patch, string modifiedBy);
		TEntity Patch(TEntity model, List<PatchDto> patches, string modifiedBy);

		Task<bool> DeleteAsync(string id);
		bool Delete(TEntity model);

		TEntity UpdateEntityMeta(TEntity entity, string modifiedBy, bool isNew = false);


		#endregion

		#region UTILITY

		TEntity Detach(TEntity model);

		#endregion
	}

	public class GenericBaseRepository<TEntity, TDbContext> : IGenericBaseRepository<TEntity>
	    where TEntity : Entities.BaseMetaExtendedEntity
		where TDbContext: DbContext
    {

	    protected readonly TDbContext _context;
	    protected Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> _orderByFn;

		public GenericBaseRepository(
			TDbContext context,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderByFn = null)
		{
			_context = context;
			_orderByFn = orderByFn;
		}

		#region >> HELPERS <<

		protected virtual IQueryable<TEntity> Query(bool inclArchived, bool asNoTracking = true)
		{
			var query = asNoTracking ? _context.Set<TEntity>().AsNoTracking() : _context.Set<TEntity>();

			if (!inclArchived)
			{
				query = query
					.Where(m => m.Archived == false);
			}

			return query;
		}

		protected virtual IOrderedQueryable<TEntity> OrderEntities(IQueryable<TEntity> query)
		{
			if (_orderByFn != null) { return _orderByFn(query); }
			//else
			return query.OrderBy(m => m.CreateDate);
		}

		public virtual TEntity UpdateEntityMeta(TEntity entity, string modifiedBy, bool isNew = false)
		{
			if (entity == null) { throw new ArgumentNullException(nameof(entity)); }

			entity.ModifiedBy = modifiedBy;
			entity.ModifyDate = DateTime.UtcNow;
			if (isNew) { entity.CreateDate = DateTime.UtcNow; }

			return entity;
		}


		#endregion

		#region >> GETTERS <<

		public virtual async Task<IList<TEntity>> GetAllAsync(bool inclArchived = false)
		{
			return await OrderEntities(Query(inclArchived)).ToListAsync();
		}

		public virtual async Task<IList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> filter,
			bool inclArchived = false)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}
			return await OrderEntities(Query(inclArchived).Where(filter)).ToListAsync();
		}

		public virtual async Task<TEntity> GetAsync(string id, bool asTracked = false)
		{
			return await Query(true, !asTracked).FirstOrDefaultAsync(m => m.Id == id);
		}

		public virtual async Task<TEntity> FindFirstAsync(Expression<Func<TEntity, bool>> filter, bool inclArchived = false, bool asTracked = false)
		{
			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}
			return await Query(inclArchived, !asTracked).FirstOrDefaultAsync(filter);
		}

		#endregion


		#region >> EXISTS <<

		public virtual async Task<bool> ExistsAsync(string id)
		{
			return await Query(true).AnyAsync(m => m.Id == id);
		}

		#endregion

		#region >> SETTERS <<

		public virtual async Task<TEntity> CreateAsync(TEntity model, string modifiedBy)
		{
			if (model == null) { throw new ArgumentNullException(nameof(model)); }

			await _context.Set<TEntity>().AddAsync(UpdateEntityMeta(model, modifiedBy, true));

			return model;
		}

		public virtual TEntity Patch(TEntity model, PatchDto patch, string modifiedBy)
		{
			if (model == null) { throw new ArgumentNullException(nameof(model)); }
			if (patch == null) { throw new ArgumentNullException(nameof(patch)); }

			return Patch(model, new List<PatchDto>() { patch }, modifiedBy);
		}

		public virtual TEntity Patch(TEntity model, List<PatchDto> patches, string modifiedBy)
		{
			if (model == null) { throw new ArgumentNullException(nameof(model)); }
			if (patches == null) { throw new ArgumentNullException(nameof(patches)); }
			if (patches.Any(m => m == null)) { throw new ArgumentException("found null patch!"); }

			foreach (var patch in patches)
			{
				//automatically process archived
				var archived = PatchArchived(model, patch, modifiedBy);
				if (!archived.processed)
				{
					switch (patch.Op)
					{
						case PatchOperation.add:
							PatchAdd(model, patch, modifiedBy);
							break;
						case PatchOperation.replace:
							PatchReplace(model, patch, modifiedBy);
							break;
						case PatchOperation.remove:
						case PatchOperation.delete:
							PatchRemove(model, patch, modifiedBy);
							break;
						default:
							throw new PatchException("Unsupported patch operation");
					}
				}

				//else
			}

			return model;
		}


		public virtual async Task<bool> DeleteAsync(string id)
		{
			return Delete(await GetAsync(id, true));
		}

		public virtual bool Delete(TEntity model)
		{
			if (model == null) { return false; }
			_context.Set<TEntity>().Remove(model);
			return true;
		}


		#endregion

		#region >> PATCHES <<

		protected virtual TEntity PatchAdd(TEntity model, PatchDto patch, string modifiedBy)
		{
			//override to handle any patch where Op == "add"
			return model;
		}

		protected virtual TEntity PatchRemove(TEntity model, PatchDto patch, string modifiedBy)
		{
			//override to handle any patch where Op == "remove" (or "delete")
			return model;
		}

		protected virtual TEntity PatchReplace(TEntity model, PatchDto patch, string modifiedBy)
		{
			//override to handle any patch where Op == "replace"
			return model;
		}

		protected (TEntity result, bool processed) PatchArchived(TEntity model, PatchDto patch, string modifiedBy)
		{
			bool processed = false;

			if (patch.Path.ToLower() == "/archived" || patch.Path.ToLower() == "/archive")
			{
				switch (patch.Op)
				{
					case PatchOperation.add:
						model.Archived = true;
						processed = true;
						break;
					case PatchOperation.remove:
					case PatchOperation.delete:
						model.Archived = false;
						processed = true;
						break;
					case PatchOperation.replace:
						if (bool.TryParse(patch.Value, out bool result))
						{
							model.Archived = result;
							processed = true;
						}
						else
						{
							throw new PatchException($"Invalid patch value for {patch.Path} - expected a boolean");
						}

						break;
				}

				if (processed)
				{
					UpdateEntityMeta(model, modifiedBy);
				}
			}

			return (model, processed);
		}
		#endregion


		#region >> UTILITY <<

		public TEntity Detach(TEntity model)
		{
			_context.Entry(model).State = EntityState.Detached;
			return model;
		}

		#endregion


		#region >> IDataRepository <<

		public async Task<int> SaveAsync()
		{
			return await _context.SaveChangesAsync();
		}

		#endregion
	}
}
