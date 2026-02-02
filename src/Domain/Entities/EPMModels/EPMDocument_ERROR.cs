using Domain.Common;
using System;

namespace Domain.Entities.EPMModels;

public class EPMDocument_ERROR : IRetryable
{
	// Mevcut Veriler
	public long Ent_ID { get; set; }
	public long EPMDocID { get; set; }
	public string StateDegeri { get; set; }
	public long idA3masterReference { get; set; }
	public string CadName { get; set; }
	public string name { get; set; }
	public string docNumber { get; set; }

	// Retry ve Loglama Alanları
	public string? LogMesaj { get; set; }
	public DateTime? LogDate { get; set; }
	public byte? EntegrasyonDurum { get; set; }
	public string? ActionType { get; set; }

	// *** DÜZELTİLEN KISIMLAR ***

	// 1. RetryCount (Normal Property)
	public int RetryCount { get; set; } = 0;

	// 2. Interface için Explicit Implementation (Interface int? istiyorsa int'e çeviriyoruz)
	// Setter'ı açtık, hata vermez artık.
	int? IRetryable.RetryCount
	{
		get => RetryCount;
		set => RetryCount = value ?? 0;
	}

	// 3. LastRetryDate (Normal Property yaptık, throw sildik)
	public DateTime? LastRetryDate { get; set; }
}