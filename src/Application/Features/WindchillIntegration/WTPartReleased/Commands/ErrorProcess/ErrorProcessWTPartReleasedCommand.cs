using Application.Features.WindchillIntegration.WTPartReleased.Commands.Process;
using Application.Interfaces.ApiService;
using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Interfaces.Generic;
using Application.Interfaces.IntegrationSettings;
using Application.Pipelines.Transaction;
using Application.Pipelines.WTPartLogging;
using AutoMapper;
using Domain.Entities;
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

namespace Application.Features.WindchillIntegration.WTPartReleased.Commands.ErrorProcess;

public class ErrorProcessWTPartReleasedCommand : IRequest<ErrorProcessWTPartReleasedResponse>, IWTPartLoggableRequest, ITransactionalRequest
{
	public string LogMessage { get; set; }
	public string ParcaState { get; set; }
	public string ParcaPartID { get; set; }
	public string ParcaPartMasterID { get; set; }
	public string ParcaName { get; set; }
	public string ParcaNumber { get; set; }
	public string ParcaVersion { get; set; }
	public string ActionType { get; set; }

	public byte EntegrasyonDurum { get; set; } = 1;

	public ErrorProcessWTPartReleasedCommand()
	{
		LogMessage = "WTPart Released işlemi başlatıldı.";
		ParcaState = "RELEASED";
		ParcaPartID = string.Empty;
		ParcaPartMasterID = string.Empty;
		ParcaName = string.Empty;
		ParcaNumber = string.Empty;
		ParcaVersion = string.Empty;
		ActionType = "ProcessWTPartReleased";
	}

	public class ErrorProcessWTPartReleasedCommandHandler : IRequestHandler<ErrorProcessWTPartReleasedCommand, ErrorProcessWTPartReleasedResponse>
	{
		private readonly IWTPartService<WTPartError> _wTPartService;
		private readonly IRetryService<WTPartError> _retryService;
		private readonly IGenericRepository<WTPartError> _genericWtpartErrorRepository;
		private readonly IGenericRepository<WTPartSentDatas> _genericWtpartSentRepository;

		private readonly IApiClientService _apiClientService;
		private readonly IStateService _stateService;
		private readonly IIntegrationSettingsService _integrationSettingsService;
		private readonly IMapper _mapper;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ErrorProcessWTPartReleasedCommandHandler> _logger;


		public ErrorProcessWTPartReleasedCommandHandler(
			IWTPartService<WTPartError> wTPartService,
			IStateService stateService,
			IMapper mapper,
			IIntegrationSettingsService integrationSettingsService,
			IHttpClientFactory httpClientFactory,
			IApiClientService apiClientService,
			IRetryService<WTPartError> retryService,
			IGenericRepository<WTPartSentDatas> genericWtpartSentRepository,
			IGenericRepository<WTPartError> genericWtpartErrorRepository,
			ILogger<ErrorProcessWTPartReleasedCommandHandler> logger)
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


		public async Task<ErrorProcessWTPartReleasedResponse> Handle(ErrorProcessWTPartReleasedCommand request, CancellationToken cancellationToken)
		{
			try
			{
				// 1. Modül ayarlarını kontrol ediyoruz.
				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
				{
					return new ErrorProcessWTPartReleasedResponse
					{
						Success = false,
						Message = "WTPartReleased modülü pasif durumda."
					};
				}

				// 2. Sıradaki işlenecek parçayı çekiyoruz ve deneme sayısını artırıyoruz
				var wtPartErrorEntity = await _retryService.GetNextAndIncrementAsync(
					e => e.ParcaState == "RELEASED",
					cancellationToken);

				if (wtPartErrorEntity == null)
				{
					return new ErrorProcessWTPartReleasedResponse
					{
						Success = false,
						Message = "Released durumunda veri bulunamadı."
					};
				}

				// 3. Maksimum deneme sayısını kontrol ediyoruz
				if (_retryService.ShouldDeleteEntity(wtPartErrorEntity))
				{
					// Maksimum deneme sayısı aşıldı, parçayı sil
					await _retryService.DeleteEntityAsync(wtPartErrorEntity, true, cancellationToken);

					return new ErrorProcessWTPartReleasedResponse
					{
						Success = true,
						Message = $"Released parça maksimum deneme sayısını ({_retryService.GetMaxRetryCount()}) aştığı için silindi."
					};
				}

				// 4. Loglama alanlarını güncelliyoruz.
				request.ParcaPartID = wtPartErrorEntity.ParcaPartID.ToString();
				request.ParcaPartMasterID = wtPartErrorEntity.ParcaPartMasterID.ToString();
				request.ParcaName = wtPartErrorEntity.ParcaName;
				request.ParcaNumber = wtPartErrorEntity.ParcaNumber;
				request.ParcaVersion = wtPartErrorEntity.ParcaVersion;

				// 5. Gerekli alanları dolduruyoruz: örneğin LogDate ve EntegrasyonDurum
				wtPartErrorEntity.LogDate = DateTime.Now;
				wtPartErrorEntity.EntegrasyonDurum = 1; // 1 = başarılı

				// 6. Rol mapping bilgisini, ProcessTagID = 1 (WTPartReleased) olarak alıyoruz.
				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(1);
				if (roleMapping == null || !roleMapping.IsActive)
				{
					// Rol mapping bulunamadı veya pasif durumda, parçayı güncelle ve bir sonraki denemede tekrar dene
					wtPartErrorEntity.LogMesaj = "WTPartReleased rol ayarı bulunamadı veya pasif durumda.";
					await _retryService.UpdateEntityAsync(wtPartErrorEntity, cancellationToken);

					return new ErrorProcessWTPartReleasedResponse
					{
						Success = false,
						Message = "WTPartReleased rol ayarı bulunamadı veya pasif durumda."
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

						return new ErrorProcessWTPartReleasedResponse
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

					return new ErrorProcessWTPartReleasedResponse
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

					return new ErrorProcessWTPartReleasedResponse
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

					return new ErrorProcessWTPartReleasedResponse
					{
						Success = false,
						Message = fullErrorMessage
					};
				}

				// Dinamik DTO'yu oluşturuyoruz. (Sadece rol ayarlarında tanımlı olan WindchillAttributes alanları alınacak)
				IDictionary<string, object> dynamicDto = new ExpandoObject();

				if (roleMapping.WindchillAttributes != null && roleMapping.WindchillAttributes.Any())
				{
					foreach (var attribute in roleMapping.WindchillAttributes)
					{
						// Windchill API cevabında attribute adının yer alıp almadığını kontrol ediyoruz.
						if (rootElement.TryGetProperty(attribute.AttributeName, out JsonElement jsonValue))
						{
							// JSON değeri string ise alınır.
							if (jsonValue.ValueKind == JsonValueKind.String)
							{
								dynamicDto[attribute.AttributeName] = jsonValue.GetString();
							}
							// Eğer jsonValue nesne veya dizi ise, gerçek nesneye deserialize ederek ekliyoruz.
							else if (jsonValue.ValueKind == JsonValueKind.Object || jsonValue.ValueKind == JsonValueKind.Array)
							{
								dynamicDto[attribute.AttributeName] = JsonSerializer.Deserialize<object>(jsonValue.GetRawText());
							}
							else
							{
								// Diğer veri tipleri için ToString() kullanılır.
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

				// 7. Rol mapping'in endpoints'lerine veriyi gönderiyoruz.
				bool allEndpointsSucceeded = true;
				StringBuilder endpointErrors = new StringBuilder();

				if (roleMapping.Endpoints != null && roleMapping.Endpoints.Any())
				{
					foreach (var endpoint in roleMapping.Endpoints)
					{
						// Müşterinin API adresini, roleMapping içerisindeki TargetApi ve Endpoint değerlerinin birleşimi olarak oluşturuyoruz.
						var targetUrl = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
						try
						{
							var client = _httpClientFactory.CreateClient("WindchillAPI");
							// Artık wtPartEntity yerine, dinamik DTO (dynamicDto) gönderilecek.
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

				// 8. İşlem sonucuna göre parçayı sil veya hata tablosunda güncelle

				#region SentdataGuncel
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
						LogMesaj = "Released işlem başarılı şekilde tamamlandı.",
						LogDate = DateTime.Now,
						ActionType = "ErrorProcessWTPartReleased",
						ActionDate = DateTime.Now
					};

					// Sent tablosuna ekle
					await _genericWtpartSentRepository.AddAsync(wtPartSentData);

					// Sonra parçayı sil
					//await _wTPartService.DeleteCancelledPartAsync(wtPartErrorEntity, permanent: false);
					await _genericWtpartErrorRepository.DeleteAsync(wtPartErrorEntity, permanent: true);

					// WTPartError tablosundan da sil
					await _retryService.DeleteEntityAsync(wtPartErrorEntity, true, cancellationToken);

					request.LogMessage = "Released işlem başarılı şekilde tamamlandı ve parça silindi.";
				}
				else
				{
					// Başarısız ise, hata mesajını güncelle
					string errorMessage = $"Released işleminde hata oluştu: {endpointErrors}";

					// WTPartError tablosundaki kaydı güncelle
					wtPartErrorEntity.LogMesaj = errorMessage;
					await _retryService.UpdateEntityAsync(wtPartErrorEntity, cancellationToken);

					request.LogMessage = "Released işleminde hata oluştu";
				}
				//if (allEndpointsSucceeded)
				//{
				//	// Başarılı ise, parçayı sil
				//	await _wTPartService.DeleteReleasedPartAsync(wtPartErrorEntity, permanent: false);

				//	// WTPartError tablosundan da sil
				//	await _retryService.DeleteEntityAsync(wtPartErrorEntity, true, cancellationToken);

				//	request.LogMessage = "Released işlem başarılı şekilde tamamlandı ve parça silindi.";
				//}
				//else
				//{
				//	// Başarısız ise, hata mesajını güncelle
				//	string errorMessage = $"Released işleminde hata oluştu: {endpointErrors}";

				//	// WTPartError tablosundaki kaydı güncelle
				//	wtPartErrorEntity.LogMesaj = errorMessage;
				//	await _retryService.UpdateEntityAsync(wtPartErrorEntity, cancellationToken);

				//	request.LogMessage = "Released işleminde hata oluştu.";
				//}
				#endregion

				// 9. Yanıt DTO'sunu oluşturuyoruz.
				var responseDto = _mapper.Map<ErrorProcessWTPartReleasedResponse>(wtPartErrorEntity);
				responseDto.Success = allEndpointsSucceeded;
				responseDto.Message = allEndpointsSucceeded
					? "Released işlem başarılı şekilde tamamlandı."
					: $"Released işleminde hata oluştu: {endpointErrors}";

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
						e => e.ParcaState == "RELEASED",
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

				return new ErrorProcessWTPartReleasedResponse
				{
					Success = false,
					Message = errorMessage
				};
			}
		}


		

	}
}
