using Domain.Entities;
using Domain.Entities.Auth;
using Security.JWT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.AuthModule;

public interface IAuthService
{
	Task<User> GetUserByEmailAsync(string email);
	string GenerateToken(User user);

	public Task DeleteOldRefreshTokens(Guid userId);
	public Task RevokeDescendantRefreshTokens(AuthRefreshToken refreshToken, string ipAddress, string reason);




	// IAuthService içerisine ekleyin:
	Task<AuthRefreshToken> CreateRefreshToken(User user, string ipAddress);
	Task<AuthRefreshToken?> GetRefreshTokenByToken(string refreshToken);
	Task<AuthRefreshToken> AddRefreshToken(AuthRefreshToken refreshToken);
	Task RevokeRefreshToken(AuthRefreshToken token, string ipAddress, string? reason = null, string? replacedByToken = null);
	Task<AuthRefreshToken> RotateRefreshToken(User user, AuthRefreshToken refreshToken, string ipAddress);
}
