using Application.Pipelines.Logging;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest, ILoggableRequest
{
	public string LogMessage { get; set; } = string.Empty;

	public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public LogoutCommandHandler(IHttpContextAccessor httpContextAccessor)
		{
			_httpContextAccessor = httpContextAccessor;
		}

		Task IRequestHandler<LogoutCommand>.Handle(LogoutCommand request, CancellationToken cancellationToken)
		{
			var userName = _httpContextAccessor.HttpContext?.User.Identity?.Name ?? "Bilinmeyen kullanıcı";
			request.LogMessage = $"Oturum sonlandırıldı. Kullanıcı: {userName}";
			return Task.FromResult(Unit.Value);
		}
	}
}
