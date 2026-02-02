using Application.Interfaces.Generic;
using Domain.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Adapters.Services;

/// <summary>
/// Retry service implementasyonu.
/// </summary>
public class RetryService<TEntity> : IRetryService<TEntity>
	where TEntity : class, IRetryable
{
	private readonly IRetryRepository<TEntity> _repository;
	private readonly IConfiguration _configuration;
	private readonly ILogger<RetryService<TEntity>> _logger;
	private readonly int _maxRetryCount;

	public RetryService(
		IRetryRepository<TEntity> repository,
		IConfiguration configuration,
		ILogger<RetryService<TEntity>> logger)
	{
		_repository = repository;
		_configuration = configuration;
		_logger = logger;

		_maxRetryCount = _configuration.GetValue<int>("RetrySettings:MaxRetryCount", 5);

		_logger.LogInformation("RetryService initialized with MaxRetryCount: {MaxRetryCount}", _maxRetryCount);
	}

	/// <summary>
	/// Sıradaki işlenecek entity'yi getirir ve deneme sayısını artırır.
	/// </summary>
	public async Task<TEntity> GetNextAndIncrementAsync(
		Expression<Func<TEntity, bool>> predicate = null,
		CancellationToken cancellationToken = default)
	{
		var entity = await _repository.GetNextEntityToRetryAsync(predicate, cancellationToken);

		if (entity == null)
		{
			_logger.LogInformation("No entity found to retry");
			return null;
		}

		var entityId = entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString() ?? "unknown";

		if (entity.RetryCount == null)
		{
			entity.RetryCount = 1;
			_logger.LogInformation("Entity {EntityId} first retry attempt", entityId);
		}
		else
		{
			entity.RetryCount++;
			_logger.LogInformation("Entity {EntityId} retry attempt {RetryCount}", entityId, entity.RetryCount);
		}

		entity.LastRetryDate = DateTime.Now;

		await _repository.UpdateAsync(entity, cancellationToken);

		return entity;
	}

	/// <summary>
	/// Entity'nin maksimum deneme sayısını aşıp aşmadığını kontrol eder.
	/// </summary>
	public bool ShouldDeleteEntity(TEntity entity)
	{
		var shouldDelete = entity.RetryCount >= _maxRetryCount;

		if (shouldDelete)
		{
			var entityId = entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString() ?? "unknown";
			_logger.LogWarning("Entity {EntityId} exceeded maximum retry count ({MaxRetryCount})", entityId, _maxRetryCount);
		}

		return shouldDelete;
	}

	/// <summary>
	/// Entity'yi siler.
	/// </summary>
	public async Task<TEntity> DeleteEntityAsync(
		TEntity entity,
		bool permanent = true,
		CancellationToken cancellationToken = default)
	{
		var entityId = entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString() ?? "unknown";
		_logger.LogInformation("Deleting entity {EntityId} (Permanent: {Permanent})", entityId, permanent);

		return await _repository.DeleteAsync(entity, permanent, cancellationToken);
	}

	/// <summary>
	/// Entity'yi günceller.
	/// </summary>
	public async Task<TEntity> UpdateEntityAsync(
		TEntity entity,
		CancellationToken cancellationToken = default)
	{
		var entityId = entity.GetType().GetProperty("Id")?.GetValue(entity)?.ToString() ?? "unknown";
		_logger.LogInformation("Updating entity {EntityId}", entityId);

		return await _repository.UpdateAsync(entity, cancellationToken);
	}

	/// <summary>
	/// Maksimum deneme sayısını döndürür.
	/// </summary>
	public int GetMaxRetryCount()
	{
		return _maxRetryCount;
	}
}