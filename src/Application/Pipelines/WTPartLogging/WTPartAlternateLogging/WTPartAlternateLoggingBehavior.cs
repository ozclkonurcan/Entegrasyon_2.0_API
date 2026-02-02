using CrossCuttingConcerns.Logging;
using CrossCuttingConcerns.Serilog;
using CrossCuttingConcerns.Serilog.Logger;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Pipelines.WTPartLogging.WTPartAlternateLogging;

public class WTPartAlternateLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>, IWTPartAlternateLoggableRequest
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly LoggerServiceBase _loggerServiceBase;

	public WTPartAlternateLoggingBehavior(IHttpContextAccessor httpContextAccessor, WTPartAlternateMsSqlLogger loggerServiceBase)
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
			Message = request.LogMesaj
		};

		try
		{
			// İşlem devam ediyor
			var response = await next();

			// Zorunlu alanlar: Ana parça ve muadil parça bilgileri
			if (request.AnaParcaPartID > 0 &&
				request.AnaParcaPartMasterID > 0 &&
				!string.IsNullOrEmpty(request.AnaParcaName) &&
				!string.IsNullOrEmpty(request.AnaParcaNumber) &&
				request.MuadilParcaPartID > 0 &&
				request.MuadilParcaMasterID > 0 &&
				!string.IsNullOrEmpty(request.MuadilParcaName) &&
				!string.IsNullOrEmpty(request.MuadilParcaNumber))
			{
				// Ek sütunlar sözlüğünü oluşturuyoruz
				var additionalColumns = new Dictionary<string, object>
				{
					{ "LogID", request.LogID },
					{ "AnaParcaState", request.AnaParcaState },
					{ "AnaParcaPartID", request.AnaParcaPartID },
					{ "AnaParcaPartMasterID", request.AnaParcaPartMasterID },
					{ "AnaParcaName", request.AnaParcaName },
					{ "AnaParcaNumber", request.AnaParcaNumber },
					{ "AnaParcaVersion", request.AnaParcaVersion },
					{ "MuadilParcaState", request.MuadilParcaState },
					{ "MuadilParcaPartID", request.MuadilParcaPartID },
					{ "MuadilParcaMasterID", request.MuadilParcaMasterID },
					{ "MuadilParcaName", request.MuadilParcaName },
					{ "MuadilParcaNumber", request.MuadilParcaNumber },
					{ "MuadilParcaVersion", request.MuadilParcaVersion },
					{ "KulAd", request.KulAd ?? logDetail.FullName },
					{ "LogMesaj", request.LogMesaj },
					{ "LogDate", request.LogDate ?? DateTime.Now },
					{ "EntegrasyonDurum", request.EntegrasyonDurum ?? 0 }
				};

				_loggerServiceBase.Info(JsonSerializer.Serialize(logDetail), additionalColumns);
			}
			// Eğer zorunlu alanlardan biri eksikse loglama yapılmayacak

			return response;
		}
		catch (Exception ex)
		{
			logDetail.Message = $"{request.LogMesaj} Hata: {ex.Message}";

			if (request.AnaParcaPartID > 0 &&
				request.AnaParcaPartMasterID > 0 &&
				!string.IsNullOrEmpty(request.AnaParcaName) &&
				!string.IsNullOrEmpty(request.AnaParcaNumber) &&
				request.MuadilParcaPartID > 0 &&
				request.MuadilParcaMasterID > 0 &&
				!string.IsNullOrEmpty(request.MuadilParcaName) &&
				!string.IsNullOrEmpty(request.MuadilParcaNumber))
			{
				var additionalColumns = new Dictionary<string, object>
				{
					{ "LogID", request.LogID },
					{ "AnaParcaState", request.AnaParcaState },
					{ "AnaParcaPartID", request.AnaParcaPartID },
					{ "AnaParcaPartMasterID", request.AnaParcaPartMasterID },
					{ "AnaParcaName", request.AnaParcaName },
					{ "AnaParcaNumber", request.AnaParcaNumber },
					{ "AnaParcaVersion", request.AnaParcaVersion },
					{ "MuadilParcaState", request.MuadilParcaState },
					{ "MuadilParcaPartID", request.MuadilParcaPartID },
					{ "MuadilParcaMasterID", request.MuadilParcaMasterID },
					{ "MuadilParcaName", request.MuadilParcaName },
					{ "MuadilParcaNumber", request.MuadilParcaNumber },
					{ "MuadilParcaVersion", request.MuadilParcaVersion },
					{ "KulAd", request.KulAd ?? logDetail.FullName },
					{ "LogMesaj", logDetail.Message },
					{ "LogDate", request.LogDate ?? DateTime.Now },
					{ "EntegrasyonDurum", 0 } // Hata durumunda 0 olarak ayarlandı
                };

				_loggerServiceBase.Error(JsonSerializer.Serialize(logDetail), additionalColumns);
			}
			throw;
		}
	}
}