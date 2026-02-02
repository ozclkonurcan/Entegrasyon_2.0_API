using Application.Features.WindchillIntegration.EPMDocumentReleased.Commands.Process;
using Application.Interfaces.ApiService;
using Application.Interfaces.Generic;
using Application.Interfaces.IntegrationSettings;
using Application.Interfaces.Mail;
using Application.Pipelines.EPMDocumentLogging; // Loglama arayüzümüz
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

namespace Application.Features.WindchillIntegration.EPMDocumentReleased.Commands.ErrorProcess;

public class ErrorProcessEPMDocumentReleasedCommand : IRequest<ErrorProcessEPMDocumentReleasedResponse>, IEPMDocumentLoggableRequest, ITransactionalRequest
{
	// IEPMDocumentLoggableRequest Gereksinimleri
	public string LogMessage { get; set; }
	public string EPMDocID { get; set; }
	public string DocNumber { get; set; }
	public string CadName { get; set; }
	public string StateDegeri { get; set; }

	public ErrorProcessEPMDocumentReleasedCommand()
	{
		LogMessage = "EPMDocument Released Error Process işlemi başlatıldı.";
		StateDegeri = "RELEASED";
		EPMDocID = string.Empty;
		DocNumber = string.Empty;
		CadName = string.Empty;
	}

	public class ErrorProcessEPMDocumentReleasedCommandHandler : IRequestHandler<ErrorProcessEPMDocumentReleasedCommand, ErrorProcessEPMDocumentReleasedResponse>
	{
		// Retry Service (WTPart'taki gibi Generic Retry yapısını kullanıyoruz)
		private readonly IRetryService<EPMDocument_ERROR> _retryService;

		private readonly IGenericRepository<EPMDocument_SENT> _genericEpmSentRepository;
		private readonly IGenericRepository<EPMDocument_ERROR> _genericEpmErrorRepository;

		private readonly IApiClientService _apiClientService;
		private readonly IIntegrationSettingsService _integrationSettingsService;
		private readonly IMapper _mapper;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ErrorProcessEPMDocumentReleasedCommandHandler> _logger;
		private readonly IMailService _mailService;

		public ErrorProcessEPMDocumentReleasedCommandHandler(
			IRetryService<EPMDocument_ERROR> retryService,
			IGenericRepository<EPMDocument_SENT> genericEpmSentRepository,
			IGenericRepository<EPMDocument_ERROR> genericEpmErrorRepository,
			IApiClientService apiClientService,
			IIntegrationSettingsService integrationSettingsService,
			IMapper mapper,
			IHttpClientFactory httpClientFactory,
			ILogger<ErrorProcessEPMDocumentReleasedCommandHandler> logger,
			IMailService mailService)
		{
			_retryService = retryService;
			_genericEpmSentRepository = genericEpmSentRepository;
			_genericEpmErrorRepository = genericEpmErrorRepository;
			_apiClientService = apiClientService;
			_integrationSettingsService = integrationSettingsService;
			_mapper = mapper;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
			_mailService = mailService;
		}

		public async Task<ErrorProcessEPMDocumentReleasedResponse> Handle(ErrorProcessEPMDocumentReleasedCommand request, CancellationToken cancellationToken)
		{
			try
			{
				// 1. Modül Kontrolü
				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
				{
					return new ErrorProcessEPMDocumentReleasedResponse { Success = false, Message = "Modül pasif." };
				}

				// 2. Sıradaki Hatalı Kaydı Çek ve RetryCount Artır
				// Not: WTPart'ta 'ParcaState' idi, burada 'StateDegeri'
				var errorEntity = await _retryService.GetNextAndIncrementAsync(
					e => e.StateDegeri == "RELEASED",
					cancellationToken);

				if (errorEntity == null)
				{
					return new ErrorProcessEPMDocumentReleasedResponse { Success = false, Message = "İşlenecek hatalı kayıt bulunamadı." };
				}

				// 3. Maksimum Deneme Sayısı Kontrolü
				if (_retryService.ShouldDeleteEntity(errorEntity))
				{
					await _retryService.DeleteEntityAsync(errorEntity, true, cancellationToken);
					return new ErrorProcessEPMDocumentReleasedResponse
					{
						Success = true,
						Message = $"Maksimum deneme sayısı ({_retryService.GetMaxRetryCount()}) aşıldığı için kayıt silindi."
					};
				}

				// 4. Loglama Bilgilerini Doldur
				request.EPMDocID = errorEntity.EPMDocID.ToString();
				request.DocNumber = errorEntity.docNumber;
				request.CadName = errorEntity.CadName;
				request.StateDegeri = errorEntity.StateDegeri;

				// 5. Rol Mapping (ProcessTagId: 3)
				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(3);
				if (roleMapping == null || !roleMapping.IsActive)
				{
					await UpdateErrorMessageAsync(errorEntity, "Rol ayarı bulunamadı.", cancellationToken);
					return new ErrorProcessEPMDocumentReleasedResponse { Success = false, Message = "Rol ayarı hatası." };
				}

				#region Windchill API & Dinamik DTO
				IDictionary<string, object> dynamicDto = new ExpandoObject();
				string errorMessage = "";

				try
				{
					string windchillUrl = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{errorEntity.EPMDocID}')";
					string windchillJson = await _apiClientService.GetAsync<string>(windchillUrl);

					if (string.IsNullOrEmpty(windchillJson) || windchillJson == "{}" || windchillJson == "null")
					{
						errorMessage = $"Belge Windchill'de bulunamadı. ID: {errorEntity.EPMDocID}";
						await UpdateErrorMessageAsync(errorEntity, errorMessage, cancellationToken);
						return new ErrorProcessEPMDocumentReleasedResponse { Success = false, Message = errorMessage };
					}

					JsonDocument jsonDoc = JsonDocument.Parse(windchillJson);
					var rootElement = jsonDoc.RootElement;

					if (rootElement.TryGetProperty("error", out JsonElement errorElement))
					{
						errorMessage = $"Windchill API Hatası: {errorElement.GetRawText()}";
						await UpdateErrorMessageAsync(errorEntity, errorMessage, cancellationToken);
						return new ErrorProcessEPMDocumentReleasedResponse { Success = false, Message = errorMessage };
					}

					// Attribute Mapping
					if (roleMapping.WindchillAttributes != null)
					{
						foreach (var attribute in roleMapping.WindchillAttributes)
						{
							if (rootElement.TryGetProperty(attribute.AttributeName, out JsonElement jsonValue))
							{
								if (jsonValue.ValueKind == JsonValueKind.String) dynamicDto[attribute.AttributeName] = jsonValue.GetString();
								else if (jsonValue.ValueKind == JsonValueKind.Number) dynamicDto[attribute.AttributeName] = jsonValue.GetDecimal();
								else dynamicDto[attribute.AttributeName] = jsonValue.ToString();
							}
							else dynamicDto[attribute.AttributeName] = null;
						}
					}
				}
				catch (Exception ex)
				{
					errorMessage = $"API Hatası: {ex.Message}";
					await UpdateErrorMessageAsync(errorEntity, errorMessage, cancellationToken);
					return new ErrorProcessEPMDocumentReleasedResponse { Success = false, Message = errorMessage };
				}
				#endregion

				// 6. Endpoint Gönderimi
				bool allEndpointsSucceeded = true;
				StringBuilder endpointErrors = new StringBuilder();

				if (roleMapping.Endpoints != null && roleMapping.Endpoints.Any())
				{
					foreach (var endpoint in roleMapping.Endpoints)
					{
						var targetUrl = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
						try
						{
							var client = _httpClientFactory.CreateClient("WindchillAPI");
							var jsonContent = JsonSerializer.Serialize(dynamicDto);
							var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
							var response = await client.PostAsync(targetUrl, content, cancellationToken);

							if (!response.IsSuccessStatusCode)
							{
								var responseBody = await response.Content.ReadAsStringAsync();
								endpointErrors.AppendLine($"Hata ({targetUrl}): {response.StatusCode} - {responseBody}");
								allEndpointsSucceeded = false;
							}
						}
						catch (Exception ex)
						{
							endpointErrors.AppendLine($"Exception ({targetUrl}): {ex.Message}");
							allEndpointsSucceeded = false;
						}
					}
				}
				else
				{
					endpointErrors.AppendLine("Endpoint tanımlı değil.");
					allEndpointsSucceeded = false;
				}

				// 7. Sonuç İşleme
				if (allEndpointsSucceeded)
				{
					// Başarılı -> Sent Tablosuna Ekle
					var sentEntity = new EPMDocument_SENT
					{
						EPMDocID = errorEntity.EPMDocID,
						StateDegeri = errorEntity.StateDegeri,
						idA3masterReference = errorEntity.idA3masterReference,
						CadName = errorEntity.CadName,
						name = errorEntity.name,
						docNumber = errorEntity.docNumber
					};

					await _genericEpmSentRepository.AddAsync(sentEntity);

					// Error tablosundan sil (RetryService üzerinden)
					await _retryService.DeleteEntityAsync(errorEntity, true, cancellationToken);

					request.LogMessage = "Hatalı kayıt başarıyla yeniden işlendi ve Sent tablosuna taşındı.";
					return new ErrorProcessEPMDocumentReleasedResponse { Success = true, Message = "Başarılı" };
				}
				else
				{
					// Başarısız -> Hata Mesajını Güncelle (RetryCount zaten başta artmıştı)
					string finalError = endpointErrors.ToString();
					await UpdateErrorMessageAsync(errorEntity, finalError, cancellationToken);

					request.LogMessage = $"Yeniden deneme başarısız: {finalError}";
					return new ErrorProcessEPMDocumentReleasedResponse { Success = false, Message = finalError };
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ErrorProcess Genel Hata");
				return new ErrorProcessEPMDocumentReleasedResponse { Success = false, Message = ex.Message };
			}
		}

		// Yardımcı Metot: Sadece hata mesajını güncellemek için
		private async Task UpdateErrorMessageAsync(EPMDocument_ERROR entity, string message, CancellationToken token)
		{
			// Eğer entity içinde LogMesaj alanı varsa güncelle
			// Reflection kullanarak veya entity tipini bilerek yapılabilir.
			// WTPartError entity'sinde LogMesaj vardı, EPMDocument_ERROR'a da eklediğini varsayıyorum.

			// entity.LogMesaj = message; 
			// entity.LogDate = DateTime.Now;

			// Generic RetryService update metodunu kullan
			await _retryService.UpdateEntityAsync(entity, token);
		}
	}
}