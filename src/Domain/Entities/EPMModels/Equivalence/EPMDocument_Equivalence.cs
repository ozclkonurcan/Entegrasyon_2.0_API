using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.EPMModels.Equivalence;

// 1. ANA TABLO (Des2_EPMDocument_Equivalence)
public class EPMDocument_Equivalence
{
	public long Ent_ID { get; set; }
	public long? MainObjectID { get; set; }
	public string? MainObjectNumber { get; set; }
	public string? MainObjectName { get; set; }
	public string? MainObjectState { get; set; }
	public string? MainObjectVersion { get; set; }

	public long? RelatedObjectID { get; set; }
	public string? RelatedObjectNumber { get; set; }
	public string? RelatedObjectName { get; set; }
	public string? RelatedObjectState { get; set; }
	public string? RelatedObjectVersion { get; set; }

	public long? LinkID { get; set; } // İlişkinin ID'si (Önemli!)
	public DateTime? LogDate { get; set; }
	public byte? EntegrasyonDurum { get; set; }
	public string? LogMesaj { get; set; }
}

// 2. LOG TABLOSU
public class EPMDocument_Equivalence_Log : EPMDocument_Equivalence
{
	// Ekstra alanlar
	public string? ActionType { get; set; }
}

// 3. SENT (GÖNDERİLENLER) TABLOSU
public class EPMDocument_Equivalence_Sent : EPMDocument_Equivalence
{
	// Base sınıf ile aynı
}