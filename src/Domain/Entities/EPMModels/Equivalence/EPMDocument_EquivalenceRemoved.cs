using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.EPMModels.Equivalence;

// 1. ANA REMOVED TABLOSU
public class EPMDocument_EquivalenceRemoved : EPMDocument_Equivalence
{
	// Yapı yukarıdakiyle aynı, inheritance kullandık
}

// 2. REMOVED SENT
public class EPMDocument_EquivalenceRemoved_Sent : EPMDocument_Equivalence
{
}

// 3. REMOVED ERROR (IRetryable Şart!)
public class EPMDocument_EquivalenceRemoved_Error : EPMDocument_Equivalence, IRetryable
{
	public int RetryCount { get; set; } = 0;

	int? IRetryable.RetryCount
	{
		get => RetryCount;
		set => RetryCount = value ?? 0;
	}

	public DateTime? LastRetryDate { get; set; }
	public string? ActionType { get; set; }
}
