using Domain.Entities.EPMModels.Equivalence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.EntegrasyonModulu.EMPDocumentServices;

public interface IEPMDocumentEquivalenceService
{
	// Eklenecek İlişkiler (EntegrasyonDurum = 1 olanlar)
	Task<EPMDocument_Equivalence> GetNextEquivalenceAsync(CancellationToken cancellationToken);

	// Silinecek İlişkiler (Logic aynı, tablo farklı)
	Task<EPMDocument_EquivalenceRemoved> GetNextEquivalenceRemovedAsync(CancellationToken cancellationToken);
}
