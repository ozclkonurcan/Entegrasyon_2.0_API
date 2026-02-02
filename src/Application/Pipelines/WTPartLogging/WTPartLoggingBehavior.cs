using CrossCuttingConcerns.Logging;
using CrossCuttingConcerns.Serilog.Logger;
using CrossCuttingConcerns.Serilog;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Application.Pipelines.WTPartLogging;

public class WTPartLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
		where TRequest : IRequest<TResponse>, IWTPartLoggableRequest
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly LoggerServiceBase _loggerServiceBase;

	public WTPartLoggingBehavior(IHttpContextAccessor httpContextAccessor, WTPartMsSqlLogger loggerServiceBase)
	{
		_httpContextAccessor = httpContextAccessor;
		_loggerServiceBase = loggerServiceBase;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		// İşlem öncesi loglama detayları
		var logDetail = new LogDetail
		{
			FullName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown",
			MethodName = next.Method.Name,
			User = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
			Message = request.LogMessage
		};

		try
		{
			// İşlem devam ediyor
			var response = await next();

			// Zorunlu alanlar: ParcaPartID, ParcaPartMasterID, ParcaName, ParcaNumber
			if (!string.IsNullOrEmpty(request.ParcaPartID) &&
				!string.IsNullOrEmpty(request.ParcaPartMasterID) &&
				!string.IsNullOrEmpty(request.ParcaName) &&
				!string.IsNullOrEmpty(request.ParcaNumber))
			{
				// Ek sütunlar sözlüğünü oluşturuyoruz (ParcaState request'ten gelecek, sabitlenmiyor)
				var additionalColumns = new Dictionary<string, object>
					{
						{ "ParcaState", request.ParcaState },
						{ "ParcaPartID", request.ParcaPartID },
						{ "ParcaPartMasterID", request.ParcaPartMasterID },
						{ "ParcaName", request.ParcaName },
						{ "ParcaNumber", request.ParcaNumber },
						{ "ParcaVersion", request.ParcaVersion },
						{ "KulAd", logDetail.FullName },
						{ "LogDate", DateTime.Now },
						{ "EntegrasyonDurum", request.EntegrasyonDurum },
						{ "LogMesaj", request.LogMessage },
						{ "ActionType", request.ActionType },
						{ "ActionDate", DateTime.Now }
					};

				_loggerServiceBase.Info(JsonSerializer.Serialize(logDetail), additionalColumns);
			}
			// Eğer zorunlu alanlardan biri eksikse loglama yapılmayacak

			return response;
		}
		catch (Exception ex)
		{
			logDetail.Message = $"{request.LogMessage} Hata: {ex.Message}";

			if (!string.IsNullOrEmpty(request.ParcaPartID) &&
				!string.IsNullOrEmpty(request.ParcaPartMasterID) &&
				!string.IsNullOrEmpty(request.ParcaName) &&
				!string.IsNullOrEmpty(request.ParcaNumber))
			{
				var additionalColumns = new Dictionary<string, object>
					{
						{ "ParcaState", request.ParcaState },
						{ "ParcaPartID", request.ParcaPartID },
						{ "ParcaPartMasterID", request.ParcaPartMasterID },
						{ "ParcaName", request.ParcaName },
						{ "ParcaNumber", request.ParcaNumber },
						{ "ParcaVersion", request.ParcaVersion },
						{ "KulAd", logDetail.FullName },
						{ "LogDate", DateTime.Now },
						{ "EntegrasyonDurum", 2 },
						{ "LogMesaj", logDetail.Message },
						{ "ActionType", request.ActionType },
						{ "ActionDate", DateTime.Now }
					};

				_loggerServiceBase.Error(JsonSerializer.Serialize(logDetail), additionalColumns);
			}
			throw;
		}
	}
}