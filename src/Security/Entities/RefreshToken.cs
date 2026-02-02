using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Security.Entities;


public class RefreshToken<TId, TUserId> : Entity<TId>
{
	public TUserId UserId { get; set; }

	public string Token { get; set; }

	public DateTime ExpirationDate { get; set; }

	public string CreatedByIp { get; set; }

	public DateTime? RevokedDate { get; set; }

	public string? RevokedByIp { get; set; }

	public string? ReplacedByToken { get; set; }

	public string? ReasonRevoked { get; set; }

	public RefreshToken()
	{
		UserId = default(TUserId);
		Token = string.Empty;
		CreatedByIp = string.Empty;
	}

	public RefreshToken(TUserId userId, string token, DateTime expirationDate, string createdByIp)
	{
		UserId = userId;
		Token = token;
		ExpirationDate = expirationDate;
		CreatedByIp = createdByIp;
	}

	public RefreshToken(TId id, TUserId userId, string token, DateTime expirationDate, string createdByIp)
		: base(id)
	{
		UserId = userId;
		Token = token;
		ExpirationDate = expirationDate;
		CreatedByIp = createdByIp;
	}
}