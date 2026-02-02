using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.EPMModels;

public class EPMDocument_CANCELLED_ERROR : IRetryable
{
	public long Ent_ID { get; set; }
	public long EPMDocID { get; set; }
	public string StateDegeri { get; set; }
	public long idA3masterReference { get; set; }
	public string CadName { get; set; }
	public string name { get; set; }
	public string docNumber { get; set; }

	// Loglama ve Retry Alanları
	public string? LogMesaj { get; set; }
	public DateTime? LogDate { get; set; }
	public byte? EntegrasyonDurum { get; set; }
	public string? ActionType { get; set; }

	// Retry Interface Implementasyonu
	public int RetryCount { get; set; } = 0;
	int? IRetryable.RetryCount
	{
		get => RetryCount;
		set => RetryCount = value ?? 0;
	}
	public DateTime? LastRetryDate { get; set; }
}
