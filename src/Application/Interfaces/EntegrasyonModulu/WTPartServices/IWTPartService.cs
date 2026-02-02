using Application.Paging;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
namespace Application.Interfaces.EntegrasyonModulu.WTPartServices;

public interface IWTPartService<TEntity> where TEntity : class
{

	Task<ICollection<WTPart>> GetState();
	Task<WTPart> GetPart(string stateType);
	Task<WTPart> GetPartID(long ParcaPartID);
	Task<WTPart> SendWTPartAsync(string stateType);
	Task SendWTPartAlternate();
	Task SendWTPartEquivalence();
	Task SendWTPartToERP(string stateType);
	    // Diğer metotlar...
    // Örneğin, GetAsync metodu vs.
	//Özel Kullanılacak olan atlttaki yukarıdakilerin hepsi rastgele deneme olanlar altakinin mantığını çözüp onu kullanıcaz

	Task<TEntity?> GetAsync(
	DbContext? context,
	Expression<Func<TEntity, bool>> predicate,
	Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
	bool withDeleted = false,
	bool enableTracking = true,
	CancellationToken cancellationToken = default);



	Task<Paginate<TEntity>> GetListAsync(
		Expression<Func<TEntity, bool>>? predicate = null,
		Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
		Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
		int index = 0,
		int size = 10,
		bool withDeleted = false,
		bool enableTracking = true,
		CancellationToken cancellationToken = default);

	Task<WTPart> DeleteAsync(DbContext? context,WTPart entity, bool permanent = false);
	Task DeleteReleasedPartAsync(TEntity wtPartEntity, bool permanent);
	Task DeleteCancelledPartAsync(TEntity wtPartEntity, bool permanent);

	Task MoveReleasedPartToErrorAsync(WTPart wtPart, string errorMessage);

	Task<List<WTPartSentDatas>> GetWTPartSentDatasAsync();
	Task<WTPartSentDatas> GetWTPartSentDataControlAsync(long ParcaPartID, long ParcaPartMasterID);

	Task UpdateReleasedPartAsync(WTPart wTPartSentDatas);
	Task<bool> ErrorRecordExistsAsync(long parcaPartID, long parcaPartMasterID);

	Task<Paginate<WTPartAllLogs>> GetWTPartAllLogsAsync(
	Expression<Func<WTPartAllLogs, bool>>? predicate = null,
	Func<IQueryable<WTPartAllLogs>, IOrderedQueryable<WTPartAllLogs>>? orderBy = null,
	Func<IQueryable<WTPartAllLogs>, IIncludableQueryable<WTPartAllLogs, object>>? include = null,
	int index = 0,
	int size = 10,
	bool withDeleted = false,
	bool enableTracking = true,
	CancellationToken cancellationToken = default);



}
