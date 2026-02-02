using Application.Interfaces.Generic;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories.Generic;

/// <summary>
/// Retry repository implementasyonu.
/// </summary>
public class RetryRepository<TEntity> : IRetryRepository<TEntity>
	where TEntity : class, IRetryable
{
	private readonly BaseDbContexts _context;
	private readonly DbSet<TEntity> _dbSet;

	public RetryRepository(BaseDbContexts context)
	{
		_context = context;
		_dbSet = _context.Set<TEntity>();
	}

	/// <summary>
	/// Sıradaki işlenecek entity'yi getirir.
	/// </summary>
	public async Task<TEntity> GetNextEntityToRetryAsync(
		Expression<Func<TEntity, bool>> predicate = null,
		CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> query = _dbSet;

		if (predicate != null)
		{
			query = query.Where(predicate);
		}

		return await query
			.OrderBy(e => e.RetryCount ?? 0)
			.ThenBy(e => e.LastRetryDate ?? DateTime.MinValue)
			.FirstOrDefaultAsync(cancellationToken);
	}

	/// <summary>
	/// Entity'yi günceller.
	/// </summary>
	public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
	{
		_dbSet.Update(entity);
		await _context.SaveChangesAsync(cancellationToken);
		return entity;
	}

	/// <summary>
	/// Entity'yi siler.
	/// </summary>
	public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = true, CancellationToken cancellationToken = default)
	{
		if (permanent)
		{
			_dbSet.Remove(entity);
		}
		else
		{
			// Soft delete işlemi için entity'de IsDeleted gibi bir alan varsa burada işaretlenebilir
			// entity.IsDeleted = true;
			_dbSet.Update(entity);
		}

		await _context.SaveChangesAsync(cancellationToken);
		return entity;
	}
}
