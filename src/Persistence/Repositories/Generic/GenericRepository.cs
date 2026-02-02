using Application.Interfaces.Generic;
using Application.Paging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories.Generic;



public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
	private readonly BaseDbContexts _context;
	public GenericRepository(BaseDbContexts context)
	{
		_context = context;
	}


	//public async Task<TEntity?> GetFirstAsync(
	//Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
	//Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
	//bool withDeleted = false,
	//bool enableTracking = true,
	//CancellationToken cancellationToken = default)
	//{
	//	IQueryable<TEntity> query = _context.Set<TEntity>();

	//	if (!enableTracking)
	//		query = query.AsNoTracking();

	//	if (!withDeleted)
	//		//query = query.Where(e => e.DeletedDate == null);

	//		if (include != null)
	//			query = include(query);

	//	if (orderBy != null)
	//		query = orderBy(query);

	//	return await query.FirstOrDefaultAsync(cancellationToken);
	//}

	public async Task<ICollection<TEntity>> GetState()
	{
		// İstersen performans için AsNoTracking() de ekleyebilirsin:
		// return await _context.Set<TEntity>().AsNoTracking().ToListAsync();
		return await _context.Set<TEntity>().ToListAsync();
	}

	public async Task<TEntity?> GetFirstAsync(
		Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
		Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
		bool withDeleted = false,
		bool enableTracking = true,
		CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> query = _context.Set<TEntity>();

		if (!enableTracking)
			query = query.AsNoTracking();

		if (!withDeleted)
			//query = query.Where(e => e.DeletedDate == null);

			if (include != null)
				query = include(query);

		// Eğer orderBy parametresi verilmemişse, varsayılan bir sıralama uygula
		if (orderBy != null)
			query = orderBy(query);
		else
		{
			// Entity'nin primary key'ini bul ve ona göre sırala
			var entityType = _context.Model.FindEntityType(typeof(TEntity));
			var primaryKey = entityType?.FindPrimaryKey()?.Properties.FirstOrDefault();

			if (primaryKey != null)
			{
				string keyName = primaryKey.Name;
				// Dynamic LINQ kullanarak property adına göre sıralama yap
				query = query.OrderBy(keyName);
			}
			else
			{
				// Primary key bulunamazsa, herhangi bir property ile sıralama yap
				// Bu sadece uyarıyı gidermek için, gerçek bir sıralama mantığı değil
				var property = entityType?.GetProperties().FirstOrDefault();
				if (property != null)
				{
					query = query.OrderBy(property.Name);
				}
			}
		}

		return await query.FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<TEntity?> GetAsync(
			Expression<Func<TEntity, bool>> predicate,
			Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
			bool withDeleted = false,
			bool enableTracking = true,
			CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> query = _context.Set<TEntity>();

		if (!enableTracking)
			query = query.AsNoTracking();

		if (!withDeleted)
			//query = query.Where(e => e.DeletedDate == null);

			if (include != null)
				query = include(query);

		return await query.FirstOrDefaultAsync(predicate, cancellationToken);
	}

	public async Task<List<TEntity>> GetListAsync(
		Expression<Func<TEntity, bool>>? predicate = null,
		Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
		Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
		bool withDeleted = false,
		bool enableTracking = true,
		CancellationToken cancellationToken = default)
	{
		try
		{

		IQueryable<TEntity> query = _context.Set<TEntity>();

		if (!enableTracking)
			query = query.AsNoTracking();

		if (!withDeleted)
			//query = query.Where(e => e.DeletedDate == null);

			if (include != null)
				query = include(query);

		if (predicate != null)
			query = query.Where(predicate);

		if (orderBy != null)
			query = orderBy(query);

		return await query.ToListAsync(cancellationToken);

		}
		catch (Exception ex)
		{

			throw;
		}
	}

	public async Task<Paginate<TEntity>> GetListPaginationAsync(
		Expression<Func<TEntity, bool>>? predicate = null,
		Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
		Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
		int index = 0,
		int size = 10,
		bool withDeleted = false,
		bool enableTracking = true,
		CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> query = _context.Set<TEntity>();

		if (!enableTracking)
			query = query.AsNoTracking();

		if (!withDeleted)
			//query = query.Where(e => e.DeletedDate == null);

			if (include != null)
				query = include(query);

		if (predicate != null)
			query = query.Where(predicate);

		if (orderBy != null)
			query = orderBy(query);

		int count = await query.CountAsync(cancellationToken);

		var items = await query.Skip(index * size).Take(size).ToListAsync(cancellationToken);

		return new Paginate<TEntity>
		{
			Items = items,
			Index = index,
			Size = size,
			Count = count,
			Pages = (int)Math.Ceiling(count / (double)size),
		};
	}

	public async Task<bool> AnyAsync(
		Expression<Func<TEntity, bool>>? predicate = null,
		bool withDeleted = false,
		CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> query = _context.Set<TEntity>();

		if (!withDeleted)
			//query = query.Where(e => e.DeletedDate == null);

			if (predicate != null)
				query = query.Where(predicate);

		return await query.AnyAsync(cancellationToken);
	}

	public async Task<int> CountAsync(
		Expression<Func<TEntity, bool>>? predicate = null,
		bool withDeleted = false,
		CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> query = _context.Set<TEntity>();

		if (!withDeleted)
			//query = query.Where(e => e.DeletedDate == null);

			if (predicate != null)
				query = query.Where(predicate);

		return await query.CountAsync(cancellationToken);
	}

	// Create Operations
	public async Task<TEntity> AddAsync(TEntity entity)
	{
		//entity.CreatedDate = DateTime.UtcNow;
		await _context.Set<TEntity>().AddAsync(entity);
		await _context.SaveChangesAsync();
		return entity;
	}

	public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities)
	{
		foreach (var entity in entities)
			//entity.CreatedDate = DateTime.UtcNow;

			await _context.Set<TEntity>().AddRangeAsync(entities);
		await _context.SaveChangesAsync();
		return entities;
	}

	// Update Operations
	public async Task<TEntity> UpdateAsync(TEntity entity)
	{
		//entity.UpdatedDate = DateTime.UtcNow;
		_context.Set<TEntity>().Update(entity);
		await _context.SaveChangesAsync();
		return entity;
	}

	public async Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities)
	{
		foreach (var entity in entities)
			//entity.UpdatedDate = DateTime.UtcNow;

			_context.Set<TEntity>().UpdateRange(entities);
		await _context.SaveChangesAsync();
		return entities;
	}

	// Delete Operations
	public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false)
	{
		if (!permanent)
		{
			//entity.DeletedDate = DateTime.UtcNow;
			_context.Set<TEntity>().Update(entity);
		}
		else
		{
			_context.Set<TEntity>().Remove(entity);
		}

		await _context.SaveChangesAsync();
		return entity;
	}

	public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false)
	{
		if (!permanent)
		{
			foreach (var entity in entities)
				//entity.DeletedDate = DateTime.UtcNow;

				_context.Set<TEntity>().UpdateRange(entities);
		}
		else
		{
			_context.Set<TEntity>().RemoveRange(entities);
		}

		await _context.SaveChangesAsync();
		return entities;
	}
}