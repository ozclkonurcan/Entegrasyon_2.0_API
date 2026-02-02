using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Generic;

/// <summary>
/// Retry işlemlerini yöneten servis interface'i.
/// </summary>
public interface IRetryService<TEntity> where TEntity : class, IRetryable
{
	/// <summary>
	/// Sıradaki işlenecek entity'yi getirir ve deneme sayısını artırır.
	/// </summary>
	Task<TEntity> GetNextAndIncrementAsync(
		Expression<Func<TEntity, bool>> predicate = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Entity'nin maksimum deneme sayısını aşıp aşmadığını kontrol eder.
	/// </summary>
	bool ShouldDeleteEntity(TEntity entity);

	/// <summary>
	/// Entity'yi siler.
	/// </summary>
	Task<TEntity> DeleteEntityAsync(
		TEntity entity,
		bool permanent = true,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Entity'yi günceller.
	/// </summary>
	Task<TEntity> UpdateEntityAsync(
		TEntity entity,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Maksimum deneme sayısını döndürür.
	/// </summary>
	int GetMaxRetryCount();
}
