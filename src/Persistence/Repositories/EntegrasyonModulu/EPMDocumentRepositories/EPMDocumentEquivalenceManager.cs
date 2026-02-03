//using Application.Interfaces.EntegrasyonModulu.EMPDocumentServices;
//using Domain.Entities.EPMModels.Equivalence;
//using Microsoft.EntityFrameworkCore;
//using Persistence.Context;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Persistence.Repositories.EntegrasyonModulu.EPMDocumentRepositories;

//public class EPMDocumentEquivalenceManager : IEPMDocumentEquivalenceService
//{
//	private readonly BaseDbContexts _context;

//	public EPMDocumentEquivalenceManager(BaseDbContexts context)
//	{
//		_context = context;
//	}

//	public async Task<EPMDocument_Equivalence> GetNextEquivalenceAsync(CancellationToken cancellationToken)
//	{
//		// EntegrasyonDurum = 1 (Trigger tarafından onaylanmış) olan İLK kaydı getir.
//		return await _context.EPMDocument_Equivalence
//			.Where(x => x.EntegrasyonDurum == 1)
//			.OrderBy(x => x.LogDate) // FIFO (İlk giren ilk çıkar)
//			.FirstOrDefaultAsync(cancellationToken);
//	}

//	public async Task<EPMDocument_EquivalenceRemoved> GetNextEquivalenceRemovedAsync(CancellationToken cancellationToken)
//	{
//		// Removed tablosunda genellikle EntegrasyonDurum 0 başlar, triggerla 1 yapılır mı?
//		// Removed senaryosunda "Onay" (Released kontrolü) gerekmez, direkt silinmiştir.
//		// O yüzden EntegrasyonDurum'a bakmadan veya 0 olanı alabiliriz.
//		// Ama standart olsun diye "İşlenmemiş" (Örn: Sent tablosunda olmayan) mantığıyla çekiyoruz.

//		return await _context.EPMDocument_EquivalenceRemoved
//			.OrderBy(x => x.LogDate)
//			.FirstOrDefaultAsync(cancellationToken);
//	}
//	}
