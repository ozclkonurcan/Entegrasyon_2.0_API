using CrossCuttingConcerns.Logging;
using CrossCuttingConcerns.Serilog;
using MediatR;
using Microsoft.AspNetCore.Http;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Pipelines.Logging;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>, ILoggableRequest
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly LoggerServiceBase _loggerServiceBase;

	public LoggingBehavior(IHttpContextAccessor httpContextAccessor, LoggerServiceBase loggerServiceBase)
	{
		_httpContextAccessor = httpContextAccessor;
		_loggerServiceBase = loggerServiceBase;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		List<LogParameter> parameters = new()
	{
		new LogParameter{Type = request.GetType().Name, Value = request},
	};

		LogDetail logDetail = new()
		{
			FullName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown",
			MethodName = next.Method.Name,
			Parameters = parameters,
			User = _httpContextAccessor.HttpContext.User.Identity?.Name ?? "?",

		};

		
		try
		{
			// İsteği işle ve sonucu döndür

			var response = await next();
			logDetail.Message = request.LogMessage; // Ensure LogMessage is set after next()
			var additionalColumns = new Dictionary<string, object>
		{
			{ "TetiklenenFonksiyon", request.GetType().Name ?? "Unknown" },
			{ "KullaniciAdi", logDetail.FullName ?? "Unknown" },
			{ "HataMesaji", logDetail.Message }
		};
			_loggerServiceBase.Info(JsonSerializer.Serialize(logDetail), additionalColumns); // Başarılı loglama
			return response;
		}
		catch (Exception ex)
		{
			// Hata durumunda loglama
			logDetail.Message = $"{request.LogMessage} Hata: {ex.Message}"; // Hata mesajını ekle
			var additionalColumns = new Dictionary<string, object>
		{
			{ "TetiklenenFonksiyon", request.GetType().Name ?? "Unknown" },
			{ "KullaniciAdi", logDetail.FullName ?? "Unknown" },
			{ "HataMesaji", logDetail.Message }
		};
			_loggerServiceBase.Error(JsonSerializer.Serialize(logDetail), additionalColumns); // Hata loglama
			throw; // Hatayı yeniden fırlat
		}
	}

	//public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	//{
	//	try
	//	{


	//	List<LogParameter> parameters = new()
	//	{
	//		new LogParameter{Type = request.GetType().Name, Value = request},
	//	};

	//	LogDetail logDetail = new()
	//	{
	//		FullName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown", 
	//		MethodName = next.Method.Name,
	//		Parameters = parameters,
	//		User = _httpContextAccessor.HttpContext.User.Identity?.Name ?? "?",
	//		Message = request.LogMessage
	//	};

	//	_loggerServiceBase.Info(JsonSerializer.Serialize(logDetail));
	//	return await next();
	//	}
	//	catch (Exception ex)
	//	{

	//		throw;
	//	}
	//}


}