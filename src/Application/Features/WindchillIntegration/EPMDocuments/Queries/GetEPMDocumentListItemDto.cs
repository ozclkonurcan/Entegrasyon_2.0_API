using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocuments.Queries;

public class GetEPMDocumentListItemDto
{
	public long Ent_ID { get; set; }
	public long? EPMDocID { get; set; }
	public string StateDegeri { get; set; }
	public string CadName { get; set; }
	public string Name { get; set; }
	public string DocNumber { get; set; }

	// Log/Sent/Error tablolarında olan alanlar (Nullable olabilir)
	public string? LogMesaj { get; set; }
	public DateTime? LogDate { get; set; }
	public int? RetryCount { get; set; } // Sadece Error tablosunda var
}
