using Application.Features.WindchillIntegration.WTPartCancelled.Commands.Process;
using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Interfaces.Generic;
using Application.Interfaces.IntegrationSettings;
using Application.Pipelines.Transaction;
using Application.Pipelines.WTPartLogging;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.WTPartModels.AlternateModels;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Application.Features.WindchillIntegration.WTPartCancelled.Commands.Process.ProcessWTPartCancelledCommand;

namespace Application.Features.WindchillIntegration.WTPartCancelled.Commands.ErrorProcess;

public class ErrorProcessWTPartCancelledCommand : IRequest<ErrorProcessWTPartCancelledResponse>, IWTPartLoggableRequest, ITransactionalRequest
{
	// Loglama için gerekli alanlar
	public string LogMessage { get; set; }
	public string ParcaState { get; set; }
	public string ParcaPartID { get; set; }
	public string ParcaPartMasterID { get; set; }
	public string ParcaName { get; set; }
	public string ParcaNumber { get; set; }
	public string ParcaVersion { get; set; }
	public string ActionType { get; set; }
	public byte EntegrasyonDurum { get; set; } = 1;

	public ErrorProcessWTPartCancelledCommand()
	{
		LogMessage = "WTPart Cancelled işlemi başlatıldı.";
		ParcaState = "CANCELLED";
		ParcaPartID = string.Empty;
		ParcaPartMasterID = string.Empty;
		ParcaName = string.Empty;
		ParcaNumber = string.Empty;
		ParcaVersion = string.Empty;
		ActionType = "ProcessWTPartCancelled";
	}

	public class ErrorProcessWTPartCancelledCommandHandler : IRequestHandler<ErrorProcessWTPartCancelledCommand, ErrorProcessWTPartCancelledResponse>
	{
		private readonly IWTPartService<WTPartError> _wTPartService;
		private readonly IRetryService<WTPartError> _retryService;
		private readonly IGenericRepository<WTPartError> _genericWtpartErrorRepository;
		private readonly IGenericRepository<WTPartSentDatas> _genericWtpartSentRepository;
		private readonly IStateService _stateService;
		private readonly IIntegrationSettingsService _integrationSettingsService;
		private readonly IMapper _mapper;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly Interfaces.ApiService.IApiClientService _apiClientService;
		private readonly ILogger<ErrorProcessWTPartCancelledCommandHandler> _logger;

		public ErrorProcessWTPartCancelledCommandHandler(
			IWTPartService<WTPartError> wTPartService,
			IStateService stateService,
			IMapper mapper,
			IIntegrationSettingsService integrationSettingsService,
			IHttpClientFactory httpClientFactory,
			Interfaces.ApiService.IApiClientService apiClientService,
			IRetryService<WTPartError> retryService,
			IGenericRepository<WTPartSentDatas> genericWtpartSentRepository,
			IGenericRepository<WTPartError> genericWtpartErrorRepository,
			ILogger<ErrorProcessWTPartCancelledCommandHandler> logger)
		{
			_wTPartService = wTPartService;
			_stateService = stateService;
			_mapper = mapper;
			_integrationSettingsService = integrationSettingsService;
			_httpClientFactory = httpClientFactory;
			_apiClientService = apiClientService;
			_retryService = retryService;
			_genericWtpartSentRepository = genericWtpartSentRepository;
			_genericWtpartErrorRepository = genericWtpartErrorRepository;
			_logger = logger;
		}

		public async Task<ErrorProcessWTPartCancelledResponse> Handle(ErrorProcessWTPartCancelledCommand request, CancellationToken cancellationToken)
		{
			try
			{
				// 1. Modül ayarlarını kontrol ediyoruz.
				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
				{
					return new ErrorProcessWTPartCancelledResponse
					{
						Success = false,
						Message = "WTPartCancelled modülü pasif durumda."
					};
				}

				// 2. Sıradaki işlenecek parçayı çekiyoruz ve deneme sayısını artırıyoruz
				var wtPartErrorEntity = await _retryService.GetNextAndIncrementAsync(
					e => e.ParcaState == "CANCELLED",
					cancellationToken);

				if (wtPartErrorEntity == null)
				{
					return new ErrorProcessWTPartCancelledResponse
					{
						Success = false,
						Message = "Cancelled durumunda veri bulunamadı."
					};
				}

				// 3. Maksimum deneme sayısını kontrol ediyoruz
				if (_retryService.ShouldDeleteEntity(wtPartErrorEntity))
				{
					// Maksimum deneme sayısı aşıldı, parçayı sil
					await _retryService.DeleteEntityAsync(wtPartErrorEntity, true, cancellationToken);

					return new ErrorProcessWTPartCancelledResponse
					{
						Success = true,
						Message = $"Cancelled parça maksimum deneme sayısını ({_retryService.GetMaxRetryCount()}) aştığı için silindi."
					};
				}

				// 4. Loglama alanlarını güncelliyoruz.
				request.ParcaPartID = wtPartErrorEntity.ParcaPartID.ToString();
				request.ParcaPartMasterID = wtPartErrorEntity.ParcaPartMasterID.ToString();
				request.ParcaName = wtPartErrorEntity.ParcaName;
				request.ParcaNumber = wtPartErrorEntity.ParcaNumber;
				request.ParcaVersion = wtPartErrorEntity.ParcaVersion;

				wtPartErrorEntity.LogDate = DateTime.Now;
				wtPartErrorEntity.EntegrasyonDurum = 1; // 1 = başarılı

				// 5. Rol mapping bilgisini, ProcessTagID = 2 (örneğin, Cancelled rolü için) çekiyoruz.
				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(2);
				if (roleMapping == null || !roleMapping.IsActive)
				{
					// Rol mapping bulunamadı veya pasif durumda, parçayı güncelle ve bir sonraki denemede tekrar dene
					wtPartErrorEntity.LogMesaj = "WTPartCancelled rol ayarı bulunamadı veya pasif durumda.";
					await _retryService.UpdateEntityAsync(wtPartErrorEntity, cancellationToken);

					return new ErrorProcessWTPartCancelledResponse
					{
						Success = false,
						Message = "WTPartCancelled rol ayarı bulunamadı veya pasif durumda."
					};
				}

				#region Dinamik Attribute Gönderimi
				string windchillUrl = $"/Windchill/servlet/odata/ProdMgmt/Parts('OR:wt.part.WTPart:{wtPartErrorEntity.ParcaPartID}')";
				string windchillJson;

				try
				{
					windchillJson = await _apiClientService.GetAsync<string>(windchillUrl);

					// Windchill API yanıtı kontrolü
					if (string.IsNullOrEmpty(windchillJson) || windchillJson == "{}" || windchillJson == "null")
					{
						string errorMessage = $"Parça Windchill'de bulunamadı. ParcaPartID: {wtPartErrorEntity.ParcaPartID}";

						// WTPartError tablosundaki kaydı güncelle
						wtPartErrorEntity.LogMesaj = errorMessage;
						await _retryService.UpdateEntityAsync(wtPartErrorEntity, cancellationToken);

						return new ErrorProcessWTPartCancelledResponse
						{
							Success = false,
							Message = errorMessage
						};
					}
				}
				catch (Exception ex)
				{
					string errorMessage = $"Windchill API hatası: {ex.Message}. ParcaPartID: {wtPartErrorEntity.ParcaPartID}";

					// WTPartError tablosundaki kaydı güncelle
					wtPartErrorEntity.LogMesaj = errorMessage;
					await _retryService.UpdateEntityAsync(wtPartErrorEntity, cancellationToken);

					return new ErrorProcessWTPartCancelledResponse
					{
						Success = false,
						Message = errorMessage
					};
				}

				// JSON parse
				JsonDocument jsonDoc;
				try
				{
					jsonDoc = JsonDocument.Parse(windchillJson);
				}
				catch (JsonException ex)
				{
					string errorMessage = $"Geçersiz JSON yanıtı. ParcaPartID: {wtPartErrorEntity.ParcaPartID}, Yanıt: {windchillJson}";

					// WTPartError tablosundaki kaydı güncelle
					wtPartErrorEntity.LogMesaj = errorMessage;
					await _retryService.UpdateEntityAsync(wtPartErrorEntity, cancellationToken);

					return new ErrorProcessWTPartCancelledResponse
					{
						Success = false,
						Message = errorMessage
					};
				}

				var rootElement = jsonDoc.RootElement;

				// Hata kontrolü
				if (rootElement.TryGetProperty("error", out JsonElement errorElement))
				{
					string errorMessage = "Bilinmeyen hata";
					if (errorElement.TryGetProperty("message", out JsonElement messageElement))
					{
						errorMessage = messageElement.GetString() ?? errorMessage;
					}

					string fullErrorMessage = $"Windchill API hatası: {errorMessage}. ParcaPartID: {wtPartErrorEntity.ParcaPartID}";

					// WTPartError tablosundaki kaydı güncelle
					wtPartErrorEntity.LogMesaj = fullErrorMessage;
					await _retryService.UpdateEntityAsync(wtPartErrorEntity, cancellationToken);

					return new ErrorProcessWTPartCancelledResponse
					{
						Success = false,
						Message = fullErrorMessage
					};
				}

				// Rol ayarlarında tanımlı olan WindchillAttributes değerleriyle dinamik DTO oluşturuyoruz.
				IDictionary<string, object> dynamicDto = new ExpandoObject();
				if (roleMapping.WindchillAttributes != null && roleMapping.WindchillAttributes.Any())
				{
					foreach (var attribute in roleMapping.WindchillAttributes)
					{
						if (rootElement.TryGetProperty(attribute.AttributeName, out JsonElement jsonValue))
						{
							if (jsonValue.ValueKind == JsonValueKind.String)
							{
								dynamicDto[attribute.AttributeName] = jsonValue.GetString();
							}
							else if (jsonValue.ValueKind == JsonValueKind.Object || jsonValue.ValueKind == JsonValueKind.Array)
							{
								dynamicDto[attribute.AttributeName] = JsonSerializer.Deserialize<object>(jsonValue.GetRawText());
							}
							else
							{
								dynamicDto[attribute.AttributeName] = jsonValue.ToString();
							}
						}
						else
						{
							dynamicDto[attribute.AttributeName] = null;
						}
					}
				}
				#endregion

				// 6. Rol mapping'in endpoints'lerine, dinamik DTO'yu gönderiyoruz.
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
								var responseContent = await response.Content.ReadAsStringAsync();
								string errorMessage = $"Endpoint {targetUrl} hatası: {response.StatusCode} - {responseContent}";
								endpointErrors.AppendLine(errorMessage);
								allEndpointsSucceeded = false;
							}
						}
						catch (HttpRequestException ex) when (ex.InnerException is SocketException socketEx && socketEx.ErrorCode == 10061)
						{
							// Bağlantı reddedildi hatası - kısa mesaj
							string errorMessage = $"API bağlantı hatası: {targetUrl} - Bağlantı kurulamadı";
							_logger.LogError(errorMessage);
							endpointErrors.AppendLine(errorMessage);
							allEndpointsSucceeded = false;
						}
						catch (HttpRequestException ex)
						{
							// Diğer HTTP hataları - kısa mesaj
							string errorMessage = $"API hatası: {targetUrl} - {ex.Message.Split('.')[0]}";
							_logger.LogError(errorMessage);
							endpointErrors.AppendLine(errorMessage);
							allEndpointsSucceeded = false;
						}
						catch (Exception ex)
						{
							// Genel hatalar - kısa mesaj
							string errorMessage = $"Genel hata: {targetUrl} - {ex.Message.Split('.')[0]}";
							_logger.LogError(ex, errorMessage);
							endpointErrors.AppendLine(errorMessage);
							allEndpointsSucceeded = false;
						}
					}
				}
				else
				{
					// Endpoint tanımlanmamışsa uyarı ekle
					string warningMessage = "Hiçbir endpoint tanımlanmamış.";
					endpointErrors.AppendLine(warningMessage);
					allEndpointsSucceeded = false;
				}

				// 7. İşlem sonucuna göre parçayı sil veya hata tablosunda güncelle
				if (allEndpointsSucceeded)
				{
					// Başarılı ise, önce Sent tablosuna ekle
					var wtPartSentData = new WTPartSentDatas
					{
						ParcaPartID = wtPartErrorEntity.ParcaPartID,
						ParcaPartMasterID = wtPartErrorEntity.ParcaPartMasterID,
						ParcaName = wtPartErrorEntity.ParcaName,
						ParcaNumber = wtPartErrorEntity.ParcaNumber,
						ParcaVersion = wtPartErrorEntity.ParcaVersion,
						ParcaState = wtPartErrorEntity.ParcaState,
						KulAd = wtPartErrorEntity.KulAd ?? "unknown",
						EntegrasyonDurum = 1, // Başarılı
						LogMesaj = "Cancelled işlem başarılı şekilde tamamlandı.",
						LogDate = DateTime.Now,
						ActionType = "ErrorProcessWTPartCancelled",
						ActionDate = DateTime.Now
					};

					// Sent tablosuna ekle
					await _genericWtpartSentRepository.AddAsync(wtPartSentData);

					// Sonra parçayı sil
					//await _wTPartService.DeleteCancelledPartAsync(wtPartErrorEntity, permanent: false);
					await _genericWtpartErrorRepository.DeleteAsync(wtPartErrorEntity, permanent: true);

					// WTPartError tablosundan da sil
					await _retryService.DeleteEntityAsync(wtPartErrorEntity, true, cancellationToken);

					request.LogMessage = "Cancelled işlem başarılı şekilde tamamlandı ve parça silindi.";
				}
				else
				{
					// Başarısız ise, hata mesajını güncelle
					string errorMessage = $"Cancelled işleminde hata oluştu: {endpointErrors}";

					// WTPartError tablosundaki kaydı güncelle
					wtPartErrorEntity.LogMesaj = errorMessage;
					await _retryService.UpdateEntityAsync(wtPartErrorEntity, cancellationToken);

					request.LogMessage = "Cancelled işleminde hata oluştu";
				}

				// 8. Sonuç DTO'sunu oluşturuyoruz.
				var responseDto = _mapper.Map<ErrorProcessWTPartCancelledResponse>(wtPartErrorEntity);
				responseDto.Success = allEndpointsSucceeded;
				responseDto.Message = allEndpointsSucceeded
					? "Cancelled işlem başarılı şekilde tamamlandı."
					: $"Cancelled işleminde hata oluştu: {endpointErrors}";

				return responseDto;
			}
			catch (Exception ex)
			{
				// Genel hata durumunda
				string errorMessage = $"İşlem sırasında beklenmeyen hata: {ex.Message}";

				try
				{
					// WTPartError tablosundan mevcut kaydı al
					var wtPartErrorEntity = await _retryService.GetNextAndIncrementAsync(
						e => e.ParcaState == "CANCELLED",
						cancellationToken);

					if (wtPartErrorEntity != null)
					{
						// Hata mesajını güncelle
						wtPartErrorEntity.LogMesaj = errorMessage;
						await _retryService.UpdateEntityAsync(wtPartErrorEntity, cancellationToken);
					}
				}
				catch (Exception ex2)
				{
					// İkinci bir hata oluştu, loglama yapılabilir
				}

				return new ErrorProcessWTPartCancelledResponse
				{
					Success = false,
					Message = errorMessage
				};
			}
		}

		#region Retry öncesi handle

		//public async Task<ErrorProcessWTPartCancelledResponse> Handle(ErrorProcessWTPartCancelledCommand request, CancellationToken cancellationToken)
		//{
		//	// 1. Modül ayarlarını kontrol ediyoruz.
		//	var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
		//	if (moduleSettings == null || moduleSettings.SettingsValue == 0)
		//	{
		//		return new ErrorProcessWTPartCancelledResponse
		//		{
		//			Success = false,
		//			Message = "WTPartCancelled modülü pasif durumda."
		//		};
		//	}

		//	// 2. CANCELLED durumundaki parçayı veritabanından çekiyoruz.
		//	var wtPartEntity = await _stateService.ERRORCANCELLED(cancellationToken);
		//	if (wtPartEntity == null)
		//	{
		//		return new ErrorProcessWTPartCancelledResponse
		//		{
		//			Success = false,
		//			Message = "Cancelled durumunda veri bulunamadı."
		//		};
		//	}

		//	// 3. Loglama alanlarını güncelliyoruz.
		//	request.ParcaPartID = wtPartEntity.ParcaPartID.ToString();
		//	request.ParcaPartMasterID = wtPartEntity.ParcaPartMasterID.ToString();
		//	request.ParcaName = wtPartEntity.ParcaName;
		//	request.ParcaNumber = wtPartEntity.ParcaNumber;
		//	request.ParcaVersion = wtPartEntity.ParcaVersion;

		//	wtPartEntity.LogDate = DateTime.Now;
		//	wtPartEntity.EntegrasyonDurum = 1; // 1 = başarılı

		//	// 4. Rol mapping bilgisini, ProcessTagID = 2 (örneğin, Cancelled rolü için) çekiyoruz.
		//	var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(2);
		//	if (roleMapping == null || !roleMapping.IsActive)
		//	{
		//		return new ErrorProcessWTPartCancelledResponse
		//		{
		//			Success = false,
		//			Message = "WTPartCancelled rol ayarı bulunamadı veya pasif durumda."
		//		};
		//	}

		//	#region Dinamik Attribute Gönderimi

		//	// Windchill API'den, ilgili parçanın detaylarını çekmek için URL oluşturuyoruz.
		//	string windchillUrl = $"/Windchill/servlet/odata/ProdMgmt/Parts('OR:wt.part.WTPart:{wtPartEntity.ParcaPartID}')";
		//	string windchillJson = await _apiClientService.GetAsync<string>(windchillUrl);
		//	using var jsonDoc = JsonDocument.Parse(windchillJson);
		//	var rootElement = jsonDoc.RootElement;

		//	// Rol ayarlarında tanımlı olan WindchillAttributes değerleriyle dinamik DTO oluşturuyoruz.
		//	IDictionary<string, object> dynamicDto = new ExpandoObject();
		//	if (roleMapping.WindchillAttributes != null && roleMapping.WindchillAttributes.Any())
		//	{
		//		foreach (var attribute in roleMapping.WindchillAttributes)
		//		{
		//			if (rootElement.TryGetProperty(attribute.AttributeName, out JsonElement jsonValue))
		//			{
		//				if (jsonValue.ValueKind == JsonValueKind.String)
		//				{
		//					dynamicDto[attribute.AttributeName] = jsonValue.GetString();
		//				}
		//				else if (jsonValue.ValueKind == JsonValueKind.Object || jsonValue.ValueKind == JsonValueKind.Array)
		//				{
		//					dynamicDto[attribute.AttributeName] = JsonSerializer.Deserialize<object>(jsonValue.GetRawText());
		//				}
		//				else
		//				{
		//					dynamicDto[attribute.AttributeName] = jsonValue.ToString();
		//				}
		//			}
		//			else
		//			{
		//				dynamicDto[attribute.AttributeName] = null;
		//			}
		//		}
		//	}
		//	#endregion

		//	// 5. Rol mapping'in endpoints'lerine, dinamik DTO'yu gönderiyoruz.
		//	bool allEndpointsSucceeded = true;
		//	if (roleMapping.Endpoints != null)
		//	{
		//		foreach (var endpoint in roleMapping.Endpoints)
		//		{
		//			var targetUrl = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
		//			try
		//			{
		//				var client = _httpClientFactory.CreateClient();
		//				var jsonContent = JsonSerializer.Serialize(dynamicDto);
		//				var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
		//				var response = await client.PostAsync(targetUrl, content, cancellationToken);
		//				if (!response.IsSuccessStatusCode)
		//				{
		//					allEndpointsSucceeded = false;
		//				}
		//			}
		//			catch (Exception ex)
		//			{
		//				allEndpointsSucceeded = false;
		//				// Hata loglama yapılabilir: ex.Message
		//			}
		//		}
		//	}

		//	// 6. Eğer tüm endpoint gönderimleri başarılı ise, parçayı siliyoruz.
		//	if (allEndpointsSucceeded)
		//	{
		//		await _wTPartService.DeleteCancelledPartAsync(wtPartEntity, permanent: false);
		//		request.LogMessage = "Cancelled işlem başarılı şekilde tamamlandı ve parça silindi.";
		//	}
		//	else
		//	{


		//		request.LogMessage = "Cancelled işleminde hata oluştu";
		//	}

		//	// 7. Sonuç DTO'sunu oluşturuyoruz.
		//	var responseDto = _mapper.Map<ErrorProcessWTPartCancelledResponse>(wtPartEntity);
		//	responseDto.Success = allEndpointsSucceeded;
		//	responseDto.Message = allEndpointsSucceeded
		//		? "Cancelled işlem başarılı şekilde tamamlandı."
		//		: "Cancelled işleminde hata oluştu, parça gönderilemedi.";
		//	return responseDto;
		//}

		#endregion
	}
}

