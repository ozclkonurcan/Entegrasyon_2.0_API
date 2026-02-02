using Application.Interfaces.AuthModule;
using Application.Interfaces.UsersModule;
using Domain.Entities;
using Domain.Entities.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Security.JWT;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories.AuthModule;

public class AuthRepository : IAuthService
{

	private readonly IUserService _userService;
	private readonly IConfiguration _configuration;

	public AuthRepository(IUserService userService, IConfiguration configuration)
	{
		_userService = userService;
		_configuration = configuration;
	}


	public async Task<User> GetUserByEmailAsync(string email)
	{
		return await _userService.GetByEmail(email);
	}

	public string GenerateToken(User user)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[]
			{
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
					new Claim(ClaimTypes.Email, user.Email),
					new Claim(ClaimTypes.Name, user.FullName),
					new Claim(ClaimTypes.Role, user.Role.ToString())
				}),
			Expires = DateTime.UtcNow.AddDays(7), // Token ömrü: 7 gün (gereksinimlere göre ayarlanabilir)
			Issuer = _configuration["Jwt:Issuer"],
			Audience = _configuration["Jwt:Audience"],
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
		};

		var token = tokenHandler.CreateToken(tokenDescriptor);
		return tokenHandler.WriteToken(token);
	}

	//public string GenerateToken(User user)
	//{
	//	var tokenHandler = new JwtSecurityTokenHandler();
	//	var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

	//	var tokenDescriptor = new SecurityTokenDescriptor
	//	{
	//		Subject = new ClaimsIdentity(new[]
	//		{
	//		new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
	//		new Claim(ClaimTypes.Email, user.Email),
	//		new Claim(ClaimTypes.Name, user.FullName)
	//	}),
	//		Expires = DateTime.UtcNow.AddDays(7), // Token süresi (örnek: 7 gün)
	//		Issuer = _configuration["Jwt:Issuer"],
	//		Audience = _configuration["Jwt:Audience"],
	//		SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
	//	};

	//	var token = tokenHandler.CreateToken(tokenDescriptor);
	//	return tokenHandler.WriteToken(token);
	//}

	
	

	public Task DeleteOldRefreshTokens(Guid userId)
	{
		throw new NotImplementedException();
	}





	public Task RevokeDescendantRefreshTokens(AuthRefreshToken refreshToken, string ipAddress, string reason)
	{
		throw new NotImplementedException();
	}

	



	public async Task<AuthRefreshToken> CreateRefreshToken(User user, string ipAddress)
	{
		var refreshToken = new AuthRefreshToken
		{
			Id = Guid.NewGuid(),
			Token = GenerateRefreshTokenString(),
			Expires = DateTime.UtcNow.AddDays(30), // Örneğin 30 gün geçerli
			Created = DateTime.UtcNow,
			CreatedByIp = ipAddress,
			UserId = user.Id
		};

		// İdeal olarak dbContext.RefreshTokens.Add(refreshToken); await dbContext.SaveChangesAsync();
		return await Task.FromResult(refreshToken);
	}

	private string GenerateRefreshTokenString()
	{
		using (var rng = RandomNumberGenerator.Create())
		{
			var randomBytes = new byte[64];
			rng.GetBytes(randomBytes);
			return Convert.ToBase64String(randomBytes);
		}
	}

	public async Task<AuthRefreshToken> AddRefreshToken(AuthRefreshToken refreshToken)
	{
		// Örneğin veritabanına ekleyin.
		return await Task.FromResult(refreshToken);
	}

	public async Task<AuthRefreshToken?> GetRefreshTokenByToken(string refreshToken)
	{
		// Örneğin refresh token’ı veritabanından çekin.
		// List içerisinden bulduğunuzu varsayalım veya dbContext.RefreshTokens.FirstOrDefaultAsync(...)
		return await Task.FromResult<AuthRefreshToken?>(null);
	}

	public async Task RevokeRefreshToken(AuthRefreshToken token, string ipAddress, string? reason = null, string? replacedByToken = null)
	{
		token.Revoked = DateTime.UtcNow;
		token.RevokedByIp = ipAddress;
		token.ReplacedByToken = replacedByToken;
		// Veritabanında güncelleyin.
		await Task.CompletedTask;
	}

	public async Task<AuthRefreshToken> RotateRefreshToken(User user, AuthRefreshToken currentRefreshToken, string ipAddress)
	{
		// Mevcut token’ı ip adresi ve neden ile iptal edip yerine yenisini oluşturun.
		await RevokeRefreshToken(currentRefreshToken, ipAddress, "Rotated", null);
		return await CreateRefreshToken(user, ipAddress);
	}
}
