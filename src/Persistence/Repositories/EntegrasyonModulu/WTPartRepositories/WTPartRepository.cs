using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Paging;
using Domain.Entities;
using Domain.Entities.IntegrationSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Persistence.Context;
using Security.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories.EntegrasyonModulu.WTPartRepositories;




public class WTPartRepository<TEntity> : IWTPartService<TEntity> where TEntity : class
{

	private readonly BaseDbContexts _dbContexts;


	public WTPartRepository(BaseDbContexts dbContexts)
	{
		_dbContexts = dbContexts;
	}




	public async Task<List<WTPartSentDatas>> GetWTPartSentDatasAsync()
	{
		return await _dbContexts.Set<WTPartSentDatas>().ToListAsync();
	}
	public async Task<WTPartSentDatas> GetWTPartSentDataControlAsync(long parcaPartID, long parcaPartMasterID)
	{
		return await _dbContexts.Set<WTPartSentDatas>()
			.FirstOrDefaultAsync(x => x.ParcaPartID == parcaPartID && x.ParcaPartMasterID == parcaPartMasterID);
	}


	public async Task UpdateReleasedPartAsync(WTPart wTPartSentDatas)
	{
		// Öncelikle hata kaydı olup olmadığını kontrol ediyoruz.
		var errorRecord = await _dbContexts.Set<WTPartSentDatas>()
			.FirstOrDefaultAsync(x => x.ParcaPartID == wTPartSentDatas.ParcaPartID &&
									  x.ParcaPartMasterID == wTPartSentDatas.ParcaPartMasterID);

		// Eğer hata kaydı zaten varsa, tekrar eklemiyoruz.
		if (errorRecord != null)
		{
			return;
		}

		// Hata kaydı yoksa, yeni bir kayıt oluşturuyoruz.
		errorRecord = new WTPartSentDatas
		{
			ParcaPartID = wTPartSentDatas.ParcaPartID,
			ParcaPartMasterID = wTPartSentDatas.ParcaPartMasterID,
			ParcaName = wTPartSentDatas.ParcaName,
			ParcaNumber = wTPartSentDatas.ParcaNumber,
			ParcaVersion = wTPartSentDatas.ParcaVersion,
			//LogDate = wTPartSentDatas.LogDate,
			// Hata mesajı veya diğer hata ile ilgili bilgileri doldurun:
			LogMesaj = "Released işleminde hata oluştu, parça gönderilemedi."
		};

		_dbContexts.Set<WTPartSentDatas>().Add(errorRecord);
		await _dbContexts.SaveChangesAsync();
	}

	public async Task<bool> ErrorRecordExistsAsync(long parcaPartID, long parcaPartMasterID)
	{
		return await _dbContexts.Set<WTPartSentDatas>()
			.AnyAsync(x => x.ParcaPartID == parcaPartID && x.ParcaPartMasterID == parcaPartMasterID);
	}


	public async Task<Paginate<WTPartAllLogs>> GetWTPartAllLogsAsync(
		Expression<Func<WTPartAllLogs, bool>>? predicate = null,
		Func<IQueryable<WTPartAllLogs>, IOrderedQueryable<WTPartAllLogs>>? orderBy = null,
		Func<IQueryable<WTPartAllLogs>, IIncludableQueryable<WTPartAllLogs, object>>? include = null,
		int index = 0,
		int size = 10,
		bool withDeleted = false,
		bool enableTracking = true,
		CancellationToken cancellationToken = default)
	{
		IQueryable<WTPartAllLogs> queryable = _dbContexts.Set<WTPartAllLogs>();

		if (!enableTracking)
			queryable = queryable.AsNoTracking();

		if (include != null)
			queryable = include(queryable);

		if (withDeleted)
			queryable = queryable.IgnoreQueryFilters();

		if (predicate != null)
			queryable = queryable.Where(predicate);

		// Sıralama her zaman yapılmalı
		if (orderBy != null)
			queryable = orderBy(queryable);
		else
			queryable = queryable.OrderByDescending(x => x.LogDate); // En son eklenen en üstte

		// Burada ToPaginateAsync metodu zaten doğru skip/take uyguluyor
		return await queryable.ToPaginateAsync(index, size, cancellationToken);
	}



	public async Task<ICollection<WTPart>> GetState()
	{
		return await _dbContexts.WTParts.ToListAsync();
	}


	public async Task<WTPart> GetPart(string stateType)
	{
		return await _dbContexts.WTParts.Where(x => x.ParcaState == stateType).FirstOrDefaultAsync();
	}


	public async Task SendWTPartAlternate()
	{
		throw new NotImplementedException();
	}

	public async Task<WTPart> SendWTPartAsync(string stateType)
	{
		WTPart wTPart = await GetPart(stateType);

		var sayac = 1;
		Console.WriteLine(sayac+" ---- "+ wTPart);
		sayac = sayac + 1;
		return wTPart;
	}

	public async Task SendWTPartEquivalence()
	{
		throw new NotImplementedException();
	}

	public async Task SendWTPartToERP(string stateType)
	{
		var sayac = 1;
		Console.WriteLine(sayac + " ---- " + stateType);
		sayac = sayac + 1;
	}

	public Task<WTPart> GetPartID(long ParcaPartID)
	{
		throw new NotImplementedException();
	}

	//Özel olanlar alttaki mantığını çözdükten sonra o tarz olamları kullanıcaz

	public async Task<TEntity?> GetAsync(DbContext? context,Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> queryable = Query();
		if (!enableTracking)
			queryable = queryable.AsNoTracking();
		if (include != null)
			queryable = include(queryable);
		if (withDeleted)
			queryable = queryable.IgnoreQueryFilters();
		return await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
	}

	public async Task<WTPart> DeleteAsync(DbContext? context, WTPart entity, bool permanent = false)
	{
		 _dbContexts.WTParts.Remove(entity);
		_dbContexts.SaveChanges();
		return entity;
	}

	public IQueryable<TEntity> Query()
	{
		return _dbContexts.Set<TEntity>();
	}


	public async Task<Paginate<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
	{
		IQueryable<TEntity> queryable = Query();
		if (!enableTracking)
			queryable = queryable.AsNoTracking();
		if (include != null)
			queryable = include(queryable);
		if (withDeleted)
			queryable = queryable.IgnoreQueryFilters();
		if (predicate != null)
			queryable = queryable.Where(predicate);
		if (orderBy != null)
			return await orderBy(queryable).ToPaginateAsync(index, size, cancellationToken);
		return await queryable.ToPaginateAsync(index, size, cancellationToken);
	}

	public async Task DeleteReleasedPartAsync(TEntity wtPartEntity, bool permanent)
	{
		// permanent flag'e göre soft veya hard delete işlemi yapılabilir.
		_dbContexts.Set<TEntity>().Remove(wtPartEntity);
		await _dbContexts.SaveChangesAsync();
	}


	public async Task DeleteCancelledPartAsync(TEntity wtPartEntity, bool permanent)
	{
		// permanent flag'e göre soft veya hard delete işlemi yapılabilir.
		_dbContexts.Set<TEntity>().Remove(wtPartEntity);
		await _dbContexts.SaveChangesAsync();
	}



	public async Task MoveReleasedPartToErrorAsync(WTPart wtPart, string errorMessage)
	{
		// Yeni hata kaydını WTPartError olarak oluşturuyoruz.
		var wtPartError = new WTPartError
		{
			LogID = wtPart.LogID,
			ParcaState = wtPart.ParcaState,
			ParcaPartID = wtPart.ParcaPartID,
			ParcaPartMasterID = wtPart.ParcaPartMasterID,
			ParcaName = wtPart.ParcaName,
			ParcaNumber = wtPart.ParcaNumber,
			ParcaVersion = wtPart.ParcaVersion,
			KulAd = wtPart.KulAd,
			LogDate = wtPart.LogDate,
			EntegrasyonDurum = wtPart.EntegrasyonDurum,
			LogMesaj = wtPart.LogMesaj,
			// Hata ile ilgili ek alanlar
			ErrorMessage = errorMessage,
			ErrorDate = DateTime.Now
		};

		await _dbContexts.WTPartErrors.AddAsync(wtPartError);
		await _dbContexts.SaveChangesAsync();

		_dbContexts.WTParts.Remove(wtPart);
		await _dbContexts.SaveChangesAsync();
	}



	
}
