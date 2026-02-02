using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Auth;

public class AuthRefreshToken
{
	public Guid Id { get; set; }
	public string Token { get; set; }
	public DateTime Expires { get; set; }
	public DateTime Created { get; set; }
	public string CreatedByIp { get; set; }
	public DateTime? Revoked { get; set; }
	public string RevokedByIp { get; set; }
	public string ReplacedByToken { get; set; }
	public bool IsExpired => DateTime.UtcNow >= Expires;
	public bool IsActive => Revoked == null && !IsExpired;

	// Kullanıcıya ait FK güncellendi
	public int UserId { get; set; }
	public User User { get; set; }
}
