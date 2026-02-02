using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common;

/// <summary>
/// Yeniden deneme (retry) mekanizması için gerekli özellikleri tanımlar.
/// </summary>
public interface IRetryable
{
	/// <summary>
	/// Entity'nin kaç kez denendiğini tutar
	/// </summary>
	int? RetryCount { get; set; }

	/// <summary>
	/// Entity'nin en son ne zaman denendiğini tutar
	/// </summary>
	DateTime? LastRetryDate { get; set; }
}