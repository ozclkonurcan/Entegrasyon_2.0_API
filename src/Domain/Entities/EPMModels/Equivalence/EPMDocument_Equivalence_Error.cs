using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.EPMModels.Equivalence;

public class EPMDocument_Equivalence_Error : EPMDocument_Equivalence, IRetryable
{
	// IRetryable Gereksinimleri
	public int RetryCount { get; set; } = 0;

	// Interface implementasyonu
	int? IRetryable.RetryCount
	{
		get => RetryCount;
		set => RetryCount = value ?? 0;
	}

	public DateTime? LastRetryDate { get; set; }
	public string? ActionType { get; set; }
}