using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common;

/// <summary>
/// Yeniden deneme (retry) mekanizması için temel sınıf.
/// </summary>
public abstract class RetryableEntity : IRetryable
{
	/// <summary>
	/// Entity'nin kaç kez denendiğini tutar
	/// </summary>
	public int? RetryCount { get; set; } = 0;

	/// <summary>
	/// Entity'nin en son ne zaman denendiğini tutar
	/// </summary>
	public DateTime? LastRetryDate { get; set; } = null;
}