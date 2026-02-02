using Application.Interfaces.AuthModule;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<LoggedResponse>
{
	public string RefreshToken { get; set; }
	public string IpAddress { get; set; }

	public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoggedResponse>
	{
		private readonly IAuthService _authService;

		public RefreshTokenCommandHandler(IAuthService authService)
		{
			_authService = authService;
		}

		public async Task<LoggedResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
		{
			var storedRefreshToken = await _authService.GetRefreshTokenByToken(request.RefreshToken);

			if (storedRefreshToken == null || !storedRefreshToken.IsActive)
				throw new UnauthorizedAccessException("Geçersiz veya süresi dolmuş refresh token.");

			var user = storedRefreshToken.User; // Navigation property doldurulmuş olmalı

			// Yenileme işlemi: refresh token'ı dönüştür.
			var newRefreshToken = await _authService.RotateRefreshToken(user, storedRefreshToken, request.IpAddress);

			// Yeni JWT token oluştur
			var newAccessToken = _authService.GenerateToken(user);

			return new LoggedResponse
			{
				Id = user.Id,
				Email = user.Email,
				FullName = user.FullName,
				Token = newAccessToken,
				RefreshToken = newRefreshToken.Token
			};
		}
	}
}
