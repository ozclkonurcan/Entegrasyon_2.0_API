using AutoMapper;
using Domain.Entities;
using Domain.Entities.Auth;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.AuthModule;

public class AuthManager : IAuthService
{
	public Task<AuthRefreshToken> AddRefreshToken(AuthRefreshToken refreshToken)
	{
		throw new NotImplementedException();
	}

	public Task<AuthRefreshToken> CreateRefreshToken(User user, string ipAddress)
	{
		throw new NotImplementedException();
	}

	public Task DeleteOldRefreshTokens(Guid userId)
	{
		throw new NotImplementedException();
	}

	public string GenerateToken(User user)
	{
		throw new NotImplementedException();
	}

	public Task<AuthRefreshToken?> GetRefreshTokenByToken(string refreshToken)
	{
		throw new NotImplementedException();
	}

	public Task<User> GetUserByEmailAsync(string email)
	{
		throw new NotImplementedException();
	}

	public Task RevokeDescendantRefreshTokens(AuthRefreshToken refreshToken, string ipAddress, string reason)
	{
		throw new NotImplementedException();
	}

	public Task RevokeRefreshToken(AuthRefreshToken token, string ipAddress, string? reason = null, string? replacedByToken = null)
	{
		throw new NotImplementedException();
	}

	public Task<AuthRefreshToken> RotateRefreshToken(User user, AuthRefreshToken refreshToken, string ipAddress)
	{
		throw new NotImplementedException();
	}
}