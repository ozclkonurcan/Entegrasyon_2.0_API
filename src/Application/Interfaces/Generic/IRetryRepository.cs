using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Generic;

/// <summary>
/// Retry mantığı için repository interface'i.
/// </summary>
public interface IRetryRepository<TEntity> where TEntity : class, IRetryable
{
	/// <summary>
	/// Sıradaki işlenecek entity'yi getirir.
	/// </summary>
	Task<TEntity> GetNextEntityToRetryAsync(
		Expression<Func<TEntity, bool>> predicate = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Entity'yi günceller.
	/// </summary>
	Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

	/// <summary>
	/// Entity'yi siler.
	/// </summary>
	Task<TEntity> DeleteAsync(TEntity entity, bool permanent = true, CancellationToken cancellationToken = default);
}