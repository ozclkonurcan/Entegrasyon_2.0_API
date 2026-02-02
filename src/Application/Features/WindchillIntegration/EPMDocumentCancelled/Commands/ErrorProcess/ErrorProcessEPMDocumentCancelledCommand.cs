using Application.Interfaces.ApiService;
using Application.Interfaces.Generic;
using Application.Interfaces.IntegrationSettings;
using Application.Interfaces.Mail;
using Application.Pipelines.EPMDocumentLogging;
using Application.Pipelines.Transaction;
using AutoMapper;
using Domain.Entities.EPMModels;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocumentCancelled.Commands.ErrorProcess;

public class ErrorProcessEPMDocumentCancelledCommand : IRequest<ErrorProcessEPMDocumentCancelledResponse>, IEPMDocumentLoggableRequest, ITransactionalRequest
{
	public string LogMessage { get; set; }
	public string EPMDocID { get; set; }
	public string DocNumber { get; set; }
	public string CadName { get; set; }
	public string StateDegeri { get; set; }

	public ErrorProcessEPMDocumentCancelledCommand()
	{
		LogMessage = "EPMDocument Cancelled Error Retry başlatıldı.";
		StateDegeri = "CANCELLED";
	}

	public class ErrorProcessEPMDocumentCancelledCommandHandler : IRequestHandler<ErrorProcessEPMDocumentCancelledCommand, ErrorProcessEPMDocumentCancelledResponse>
	{
		private readonly IRetryService<EPMDocument_CANCELLED_ERROR> _retryService;
		private readonly IGenericRepository<EPMDocument_CANCELLED_SENT> _sentRepository;

		private readonly IApiClientService _apiClientService;
		private readonly IIntegrationSettingsService _integrationSettingsService;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ErrorProcessEPMDocumentCancelledCommandHandler> _logger;

		public ErrorProcessEPMDocumentCancelledCommandHandler(
			IRetryService<EPMDocument_CANCELLED_ERROR> retryService,
			IGenericRepository<EPMDocument_CANCELLED_SENT> sentRepository,
			IApiClientService apiClientService,
			IIntegrationSettingsService integrationSettingsService,
			IHttpClientFactory httpClientFactory,
			ILogger<ErrorProcessEPMDocumentCancelledCommandHandler> logger)
		{
			_retryService = retryService;
			_sentRepository = sentRepository;
			_apiClientService = apiClientService;
			_integrationSettingsService = integrationSettingsService;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
		}

		public async Task<ErrorProcessEPMDocumentCancelledResponse> Handle(ErrorProcessEPMDocumentCancelledCommand request, CancellationToken cancellationToken)
		{
			try
			{
				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
					return new ErrorProcessEPMDocumentCancelledResponse { Success = false, Message = "Modül pasif." };

				// 1. Sıradaki Hatalı Kaydı Çek (RetryCount artar)
				var errorEntity = await _retryService.GetNextAndIncrementAsync(e => e.StateDegeri == "CANCELLED", cancellationToken);
				if (errorEntity == null)
					return new ErrorProcessEPMDocumentCancelledResponse { Success = false, Message = "Hatalı kayıt yok." };

				// 2. Limit Kontrolü
				if (_retryService.ShouldDeleteEntity(errorEntity))
				{
					await _retryService.DeleteEntityAsync(errorEntity, true, cancellationToken);
					return new ErrorProcessEPMDocumentCancelledResponse { Success = true, Message = "Limit aşıldı, silindi." };
				}

				// 3. Log Bilgileri
				request.EPMDocID = errorEntity.EPMDocID.ToString();
				request.DocNumber = errorEntity.docNumber;
				request.CadName = errorEntity.CadName;

				// 4. Rol Mapping (ProcessTagId: 4 - Cancelled)
				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(4);
				if (roleMapping == null || !roleMapping.IsActive)
				{
					errorEntity.LogMesaj = "Rol ayarı yok.";
					await _retryService.UpdateEntityAsync(errorEntity, cancellationToken);
					return new ErrorProcessEPMDocumentCancelledResponse { Success = false, Message = "Rol ayarı yok." };
				}

				#region API İşlemleri
				IDictionary<string, object> dynamicDto = new ExpandoObject();
				try
				{
					string url = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{errorEntity.EPMDocID}')";
					string json = await _apiClientService.GetAsync<string>(url);

					if (string.IsNullOrEmpty(json) || json == "{}") throw new Exception("Windchill verisi boş.");

					var root = JsonDocument.Parse(json).RootElement;
					if (root.TryGetProperty("error", out _)) throw new Exception("Windchill API hatası.");

					if (roleMapping.WindchillAttributes != null)
					{
						foreach (var attr in roleMapping.WindchillAttributes)
						{
							if (root.TryGetProperty(attr.AttributeName, out JsonElement val))
								dynamicDto[attr.AttributeName] = val.ValueKind == JsonValueKind.String ? val.GetString() : val.ToString();
							else
								dynamicDto[attr.AttributeName] = null;
						}
					}
				}
				catch (Exception ex)
				{
					errorEntity.LogMesaj = ex.Message;
					await _retryService.UpdateEntityAsync(errorEntity, cancellationToken);
					return new ErrorProcessEPMDocumentCancelledResponse { Success = false, Message = ex.Message };
				}
				#endregion

				// 5. Endpoint Gönderimi
				bool success = true;
				StringBuilder errors = new StringBuilder();

				if (roleMapping.Endpoints != null)
				{
					foreach (var endpoint in roleMapping.Endpoints)
					{
						var url = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
						try
						{
							var client = _httpClientFactory.CreateClient("WindchillAPI");
							var content = new StringContent(JsonSerializer.Serialize(dynamicDto), Encoding.UTF8, "application/json");
							var resp = await client.PostAsync(url, content, cancellationToken);
							if (!resp.IsSuccessStatusCode)
							{
								errors.AppendLine($"Hata ({url}): {resp.StatusCode}");
								success = false;
							}
						}
						catch (Exception ex) { errors.AppendLine(ex.Message); success = false; }
					}
				}

				// 6. Sonuç
				if (success)
				{
					var sent = new EPMDocument_CANCELLED_SENT
					{
						EPMDocID = errorEntity.EPMDocID,
						StateDegeri = errorEntity.StateDegeri,
						idA3masterReference = errorEntity.idA3masterReference,
						CadName = errorEntity.CadName,
						name = errorEntity.name,
						docNumber = errorEntity.docNumber
					};
					await _sentRepository.AddAsync(sent);
					await _retryService.DeleteEntityAsync(errorEntity, true, cancellationToken);

					request.LogMessage = "Hatalı kayıt başarıyla yeniden işlendi.";
					return new ErrorProcessEPMDocumentCancelledResponse { Success = true, Message = "Başarılı" };
				}
				else
				{
					errorEntity.LogMesaj = errors.ToString();
					await _retryService.UpdateEntityAsync(errorEntity, cancellationToken);
					request.LogMessage = "Tekrar deneme başarısız.";
					return new ErrorProcessEPMDocumentCancelledResponse { Success = false, Message = errors.ToString() };
				}
			}
			catch (Exception ex)
			{
				return new ErrorProcessEPMDocumentCancelledResponse { Success = false, Message = ex.Message };
			}
		}
	}
}