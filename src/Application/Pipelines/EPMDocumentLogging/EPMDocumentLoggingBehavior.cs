using CrossCuttingConcerns.Logging;
using CrossCuttingConcerns.Serilog;
using CrossCuttingConcerns.Serilog.Logger;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace Application.Pipelines.EPMDocumentLogging;

public class EPMDocumentLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>, IEPMDocumentLoggableRequest
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly EPMDocumentMsSqlLogger _loggerServiceBase; // Kendi Logger sınıfımız

	public EPMDocumentLoggingBehavior(IHttpContextAccessor httpContextAccessor, EPMDocumentMsSqlLogger loggerServiceBase)
	{
		_httpContextAccessor = httpContextAccessor;
		_loggerServiceBase = loggerServiceBase;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		// 1. Kullanıcı Bilgisini Güvenli Al (Background Service Hatasını Önler)
		string fullName = "System/Background";
		string userName = "System";

		if (_httpContextAccessor.HttpContext?.User != null)
		{
			fullName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
			userName = _httpContextAccessor.HttpContext.User.Identity?.Name ?? "Unknown";
		}

		var logDetail = new LogDetail
		{
			FullName = fullName,
			MethodName = next.Method.Name,
			User = userName,
			Message = request.LogMessage
		};

		try
		{
			var response = await next();

			// 2. Başarılı Durum Loglaması
			var additionalColumns = new Dictionary<string, object>
			{
				{ "EPMDocID", request.EPMDocID },
				{ "DocNumber", request.DocNumber },
				{ "CadName", request.CadName },
				{ "StateDegeri", request.StateDegeri },
				{ "LogMesaj", request.LogMessage },
				{ "KulAd", logDetail.FullName },
				{ "LogDate", DateTime.Now },
				{ "EntegrasyonDurum", 1 } // 1: Başarılı
            };

			_loggerServiceBase.Info(JsonSerializer.Serialize(logDetail), additionalColumns);

			return response;
		}
		catch (Exception ex)
		{
			// 3. Hata Durum Loglaması
			logDetail.Message = $"{request.LogMessage} Hata: {ex.Message}";

			var additionalColumns = new Dictionary<string, object>
			{
				{ "EPMDocID", request.EPMDocID },
				{ "DocNumber", request.DocNumber },
				{ "CadName", request.CadName },
				{ "StateDegeri", request.StateDegeri },
				{ "LogMesaj", logDetail.Message },
				{ "KulAd", logDetail.FullName },
				{ "LogDate", DateTime.Now },
				{ "EntegrasyonDurum", 2 } // 2: Hatalı
            };

			_loggerServiceBase.Error(JsonSerializer.Serialize(logDetail), additionalColumns);
			throw;
		}
	}
}