using Application.Features.WindchillIntegration.WTPartAlternateLink.Commands.Process;
using Application.Interfaces.ApiService;
using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Interfaces.Generic;
using Application.Interfaces.IntegrationSettings;
using Application.Pipelines.Transaction;
using Application.Pipelines.WTPartLogging.WTPartAlternateLogging;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.WTPartModels.AlternateModels;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartAlternateLink.Commands.ErrorProcess;

public class ErrorProcessWTPartAlternateLinkCommand : IRequest<ErrorProcessWTPartAlternateLinkResponse>, IWTPartAlternateLoggableRequest, ITransactionalRequest
{

	// LogMessage property'sini LogMesaj'a yönlendiriyoruz
	public string LogMessage
	{
		get => LogMesaj;
		set => LogMesaj = value;
	}

	public int LogID { get; set; }
	public string AnaParcaState { get; set; }
	public long AnaParcaPartID { get; set; }
	public long AnaParcaPartMasterID { get; set; }
	public string AnaParcaName { get; set; }
	public string AnaParcaNumber { get; set; }
	public string AnaParcaVersion { get; set; }
	public string MuadilParcaState { get; set; }
	public long MuadilParcaPartID { get; set; }
	public long MuadilParcaMasterID { get; set; }
	public string MuadilParcaName { get; set; }
	public string MuadilParcaNumber { get; set; }
	public string MuadilParcaVersion { get; set; }
	public string KulAd { get; set; }
	public string LogMesaj { get; set; } = "WTPart AlternateLink hata işlemi başlatıldı.";
	public DateTime? LogDate { get; set; } = DateTime.Now;
	public byte? EntegrasyonDurum { get; set; } = 1;

	public class ErrorProcessWTPartAlternateLinkCommandHandler : IRequestHandler<ErrorProcessWTPartAlternateLinkCommand, ErrorProcessWTPartAlternateLinkResponse>
	{
		private readonly IGenericRepository<WTPartAlternateLinkErrorEntegration> _genericWtpartAlternateErrorRepository;
		private readonly IGenericRepository<WTPartAlternateLinkLogEntegration> _genericWtpartAlternateLogRepository;
		private readonly IGenericRepository<WTPartAlternateLinkSentEntegration> _genericWtpartAlternateSentRepository;
		private readonly IRetryService<WTPartAlternateLinkErrorEntegration> _retryService;
		private readonly IStateService _stateService;
		private readonly IIntegrationSettingsService _integrationSettingsService;
		private readonly IMapper _mapper;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly Interfaces.ApiService.IApiClientService _apiClientService;
		private readonly ILogger<ErrorProcessWTPartAlternateLinkCommandHandler> _logger;

		public ErrorProcessWTPartAlternateLinkCommandHandler(
			IGenericRepository<WTPartAlternateLinkErrorEntegration> genericWtpartAlternateErrorRepository,
			IGenericRepository<WTPartAlternateLinkLogEntegration> genericWtpartAlternateLogRepository,
			IGenericRepository<WTPartAlternateLinkSentEntegration> genericWtpartAlternateSentRepository,
			IStateService stateService,
			IIntegrationSettingsService integrationSettingsService,
			IMapper mapper,
			IHttpClientFactory httpClientFactory,
			IApiClientService apiClientService,
			ILogger<ErrorProcessWTPartAlternateLinkCommandHandler> logger,
			IRetryService<WTPartAlternateLinkErrorEntegration> retryService)
		{
			_genericWtpartAlternateErrorRepository = genericWtpartAlternateErrorRepository;
			_genericWtpartAlternateLogRepository = genericWtpartAlternateLogRepository;
			_genericWtpartAlternateSentRepository = genericWtpartAlternateSentRepository;
			_stateService = stateService;
			_integrationSettingsService = integrationSettingsService;
			_mapper = mapper;
			_httpClientFactory = httpClientFactory;
			_apiClientService = apiClientService;
			_logger = logger;
			_retryService = retryService;
		}

		public async Task<ErrorProcessWTPartAlternateLinkResponse> Handle(ErrorProcessWTPartAlternateLinkCommand request, CancellationToken cancellationToken)
		{
			try
			{
				// 1. Modül ayarlarını kontrol ediyoruz
				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
				{
					return new ErrorProcessWTPartAlternateLinkResponse
					{
						Success = false,
						Message = "Muadil modülü pasif durumda."
					};
				}

				// 2. Hata tablosundan işlenecek ilk kaydı alıyoruz
				//var wtPartAlternateErrorEntity = await _genericWtpartAlternateErrorRepository.GetFirstAsync(cancellationToken: cancellationToken);
				var wtPartAlternateErrorEntity = await _retryService.GetNextAndIncrementAsync(cancellationToken: cancellationToken);
				if (wtPartAlternateErrorEntity == null)
				{
					return new ErrorProcessWTPartAlternateLinkResponse
					{
						Success = false,
						Message = "Hata tablosunda işlenecek muadil parça bulunamadı."
					};
				}

				// 3. Loglama alanlarını güncelliyoruz
				request.LogID = wtPartAlternateErrorEntity.LogID;
				request.AnaParcaState = wtPartAlternateErrorEntity.AnaParcaState;
				request.AnaParcaPartID = wtPartAlternateErrorEntity.AnaParcaPartID;
				request.AnaParcaPartMasterID = wtPartAlternateErrorEntity.AnaParcaPartMasterID;
				request.AnaParcaName = wtPartAlternateErrorEntity.AnaParcaName;
				request.AnaParcaNumber = wtPartAlternateErrorEntity.AnaParcaNumber;
				request.AnaParcaVersion = wtPartAlternateErrorEntity.AnaParcaVersion;
				request.MuadilParcaState = wtPartAlternateErrorEntity.MuadilParcaState;
				request.MuadilParcaPartID = wtPartAlternateErrorEntity.MuadilParcaPartID;
				request.MuadilParcaMasterID = wtPartAlternateErrorEntity.MuadilParcaMasterID;
				request.MuadilParcaName = wtPartAlternateErrorEntity.MuadilParcaName;
				request.MuadilParcaNumber = wtPartAlternateErrorEntity.MuadilParcaNumber;
				request.MuadilParcaVersion = wtPartAlternateErrorEntity.MuadilParcaVersion;
				request.KulAd = wtPartAlternateErrorEntity.KulAd;
				request.LogDate = DateTime.Now;
				request.EntegrasyonDurum = 1; // İşlemde

				wtPartAlternateErrorEntity.LogDate = DateTime.Now;
				wtPartAlternateErrorEntity.EntegrasyonDurum = 1; // 1 = başarılı

				// 4. Rol mapping bilgisini, ProcessTagID = 5 (WTPartalternate) olarak alıyoruz
				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(5);
				if (roleMapping == null || !roleMapping.IsActive)
				{
					_logger.LogWarning("Muadil rol ayarı bulunamadı veya pasif durumda. Hata kaydı korunuyor.");
					return new ErrorProcessWTPartAlternateLinkResponse
					{
						Success = false,
						Message = "Muadil rol ayarı bulunamadı veya pasif durumda."
					};
				}

				#region Dinamik Attribute Gönderimi
				IDictionary<string, object> dynamicDto = new ExpandoObject();
				bool windchillApiSuccess = false;
				string windchillErrorMessage = string.Empty;

				try
				{
					// Ana parça ve muadil parça için URL'leri oluşturuyoruz
					string anaParcaUrl = $"/Windchill/servlet/odata/ProdMgmt/Parts('OR:wt.part.WTPart:{wtPartAlternateErrorEntity.AnaParcaPartID}')";
					string muadilParcaUrl = $"/Windchill/servlet/odata/ProdMgmt/Parts('OR:wt.part.WTPart:{wtPartAlternateErrorEntity.MuadilParcaPartID}')";

					_logger.LogInformation("Ana Parça API isteği: {Url}", anaParcaUrl);
					_logger.LogInformation("Muadil Parça API isteği: {Url}", muadilParcaUrl);

					// Ana parça bilgilerini al
					string anaParcaJson = await _apiClientService.GetAsync<string>(anaParcaUrl);
					_logger.LogInformation("Ana Parça API yanıtı: {Response}", anaParcaJson);

					// Muadil parça bilgilerini al
					string muadilParcaJson = await _apiClientService.GetAsync<string>(muadilParcaUrl);
					_logger.LogInformation("Muadil Parça API yanıtı: {Response}", muadilParcaJson);

					// Ana parça API yanıtı kontrolü
					if (string.IsNullOrEmpty(anaParcaJson) || anaParcaJson == "{}" || anaParcaJson == "null")
					{
						windchillErrorMessage = $"Ana parça Windchill'de bulunamadı. ParcaPartID: {wtPartAlternateErrorEntity.AnaParcaPartID}";
						_logger.LogWarning(windchillErrorMessage);

						// Hata kaydını güncelle ve koru
						wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;
						//await _genericWtpartAlternateErrorRepository.UpdateAsync(wtPartAlternateErrorEntity);
						await _retryService.UpdateEntityAsync(wtPartAlternateErrorEntity, cancellationToken);

						return new ErrorProcessWTPartAlternateLinkResponse
						{
							Success = false,
							Message = windchillErrorMessage
						};
					}

					// Muadil parça API yanıtı kontrolü
					if (string.IsNullOrEmpty(muadilParcaJson) || muadilParcaJson == "{}" || muadilParcaJson == "null")
					{
						windchillErrorMessage = $"Muadil parça Windchill'de bulunamadı. ParcaPartID: {wtPartAlternateErrorEntity.MuadilParcaPartID}";
						_logger.LogWarning(windchillErrorMessage);

						// Hata kaydını güncelle ve koru
						wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;
						//await _genericWtpartAlternateErrorRepository.UpdateAsync(wtPartAlternateErrorEntity);
						await _retryService.UpdateEntityAsync(wtPartAlternateErrorEntity, cancellationToken);
						return new ErrorProcessWTPartAlternateLinkResponse
						{
							Success = false,
							Message = windchillErrorMessage
						};
					}

					// Ana parça JSON parse
					JsonDocument anaParcaJsonDoc;
					try
					{
						anaParcaJsonDoc = JsonDocument.Parse(anaParcaJson);
					}
					catch (JsonException ex)
					{
						windchillErrorMessage = $"Ana parça için geçersiz JSON yanıtı. ParcaPartID: {wtPartAlternateErrorEntity.AnaParcaPartID}, Yanıt: {anaParcaJson}";
						_logger.LogError(ex, windchillErrorMessage);

						// Hata kaydını güncelle ve koru
						wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;
						//await _genericWtpartAlternateErrorRepository.UpdateAsync(wtPartAlternateErrorEntity);
						await _retryService.UpdateEntityAsync(wtPartAlternateErrorEntity, cancellationToken);

						return new ErrorProcessWTPartAlternateLinkResponse
						{
							Success = false,
							Message = windchillErrorMessage
						};
					}

					// Muadil parça JSON parse
					JsonDocument muadilParcaJsonDoc;
					try
					{
						muadilParcaJsonDoc = JsonDocument.Parse(muadilParcaJson);
					}
					catch (JsonException ex)
					{
						windchillErrorMessage = $"Muadil parça için geçersiz JSON yanıtı. ParcaPartID: {wtPartAlternateErrorEntity.MuadilParcaPartID}, Yanıt: {muadilParcaJson}";
						_logger.LogError(ex, windchillErrorMessage);

						// Hata kaydını güncelle ve koru
						wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;
						//await _genericWtpartAlternateErrorRepository.UpdateAsync(wtPartAlternateErrorEntity);
						await _retryService.UpdateEntityAsync(wtPartAlternateErrorEntity, cancellationToken);
						return new ErrorProcessWTPartAlternateLinkResponse
						{
							Success = false,
							Message = windchillErrorMessage
						};
					}

					var anaParcaRootElement = anaParcaJsonDoc.RootElement;
					var muadilParcaRootElement = muadilParcaJsonDoc.RootElement;

					// Ana parça hata kontrolü
					if (anaParcaRootElement.TryGetProperty("error", out JsonElement anaParcaErrorElement))
					{
						string errorMessage = "Bilinmeyen hata";
						if (anaParcaErrorElement.TryGetProperty("message", out JsonElement messageElement))
						{
							errorMessage = messageElement.GetString() ?? errorMessage;
						}

						windchillErrorMessage = $"Ana parça Windchill API hatası: {errorMessage}. ParcaPartID: {wtPartAlternateErrorEntity.AnaParcaPartID}";
						_logger.LogWarning(windchillErrorMessage);

						// Hata kaydını güncelle ve koru
						wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;
						//await _genericWtpartAlternateErrorRepository.UpdateAsync(wtPartAlternateErrorEntity);
						await _retryService.UpdateEntityAsync(wtPartAlternateErrorEntity, cancellationToken);
						return new ErrorProcessWTPartAlternateLinkResponse
						{
							Success = false,
							Message = windchillErrorMessage
						};
					}

					// Muadil parça hata kontrolü
					if (muadilParcaRootElement.TryGetProperty("error", out JsonElement muadilParcaErrorElement))
					{
						string errorMessage = "Bilinmeyen hata";
						if (muadilParcaErrorElement.TryGetProperty("message", out JsonElement messageElement))
						{
							errorMessage = messageElement.GetString() ?? errorMessage;
						}

						windchillErrorMessage = $"Muadil parça Windchill API hatası: {errorMessage}. ParcaPartID: {wtPartAlternateErrorEntity.MuadilParcaPartID}";
						_logger.LogWarning(windchillErrorMessage);

						// Hata kaydını güncelle ve koru
						wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;
						//await _genericWtpartAlternateErrorRepository.UpdateAsync(wtPartAlternateErrorEntity);
						await _retryService.UpdateEntityAsync(wtPartAlternateErrorEntity, cancellationToken);
						return new ErrorProcessWTPartAlternateLinkResponse
						{
							Success = false,
							Message = windchillErrorMessage
						};
					}

					// Rol ayarlarında tanımlı olan WindchillAttributes değerleriyle dinamik DTO oluşturuyoruz
					if (roleMapping.WindchillAttributes != null && roleMapping.WindchillAttributes.Any())
					{
						// Ana parça bilgilerini ekle
						foreach (var attribute in roleMapping.WindchillAttributes)
						{
							if (anaParcaRootElement.TryGetProperty(attribute.AttributeName, out JsonElement anaParcaJsonValue))
							{
								string attributeKey = attribute.AttributeName;
								if (anaParcaJsonValue.ValueKind == JsonValueKind.String)
								{
									dynamicDto[attributeKey] = anaParcaJsonValue.GetString();
								}
								else if (anaParcaJsonValue.ValueKind == JsonValueKind.Object || anaParcaJsonValue.ValueKind == JsonValueKind.Array)
								{
									dynamicDto[attributeKey] = JsonSerializer.Deserialize<object>(anaParcaJsonValue.GetRawText());
								}
								else
								{
									dynamicDto[attributeKey] = anaParcaJsonValue.ToString();
								}
							}
							else
							{
								dynamicDto[attribute.AttributeName] = null;
							}
						}

						// Muadil parça bilgilerini ekle (Alternates dizisi olarak)
						var alternatePart = new Dictionary<string, object>();
						foreach (var attribute in roleMapping.WindchillAttributes)
						{
							if (muadilParcaRootElement.TryGetProperty(attribute.AttributeName, out JsonElement muadilParcaJsonValue))
							{
								string attributeKey = attribute.AttributeName;
								if (muadilParcaJsonValue.ValueKind == JsonValueKind.String)
								{
									alternatePart[attributeKey] = muadilParcaJsonValue.GetString();
								}
								else if (muadilParcaJsonValue.ValueKind == JsonValueKind.Object || muadilParcaJsonValue.ValueKind == JsonValueKind.Array)
								{
									alternatePart[attributeKey] = JsonSerializer.Deserialize<object>(muadilParcaJsonValue.GetRawText());
								}
								else
								{
									alternatePart[attributeKey] = muadilParcaJsonValue.ToString();
								}
							}
							else
							{
								alternatePart[attribute.AttributeName] = null;
							}
						}

						// Muadil parçayı ana parçanın altına ekle
						dynamicDto["Alternates"] = new List<Dictionary<string, object>> { alternatePart };
					}

					// Alternatif link bilgilerini de ekle
					dynamicDto["LogID"] = wtPartAlternateErrorEntity.LogID;
					dynamicDto["LogDate"] = wtPartAlternateErrorEntity.LogDate;
					dynamicDto["KulAd"] = wtPartAlternateErrorEntity.KulAd;
					dynamicDto["LogMesaj"] = wtPartAlternateErrorEntity.LogMesaj;
					dynamicDto["EntegrasyonDurum"] = wtPartAlternateErrorEntity.EntegrasyonDurum;

					windchillApiSuccess = true;
				}
				catch (Exception ex)
				{
					windchillErrorMessage = string.IsNullOrEmpty(windchillErrorMessage)
						? $"Windchill API hatası: {ex.Message}. Ana ParcaPartID: {wtPartAlternateErrorEntity.AnaParcaPartID}, Muadil ParcaPartID: {wtPartAlternateErrorEntity.MuadilParcaPartID}"
						: windchillErrorMessage;

					_logger.LogError(ex, windchillErrorMessage);

					// Hata kaydını güncelle ve koru
					wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
					wtPartAlternateErrorEntity.LogDate = DateTime.Now;
					//await _genericWtpartAlternateErrorRepository.UpdateAsync(wtPartAlternateErrorEntity);
					await _retryService.UpdateEntityAsync(wtPartAlternateErrorEntity, cancellationToken);
					return new ErrorProcessWTPartAlternateLinkResponse
					{
						Success = false,
						Message = windchillErrorMessage
					};
				}

				// Windchill API'den veri alınamadıysa, işlemi sonlandır
				if (!windchillApiSuccess)
				{
					return new ErrorProcessWTPartAlternateLinkResponse
					{
						Success = false,
						Message = windchillErrorMessage
					};
				}
				#endregion

				// 5. Rol mapping'in endpoints'lerine, dinamik DTO'yu gönderiyoruz
				bool allEndpointsSucceeded = true;
				StringBuilder endpointErrors = new StringBuilder();

				if (roleMapping.Endpoints != null && roleMapping.Endpoints.Any())
				{
					foreach (var endpoint in roleMapping.Endpoints)
					{
						var targetUrl = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
						try
						{
							_logger.LogInformation("Endpoint isteği gönderiliyor: {Url}", targetUrl);

							var client = _httpClientFactory.CreateClient("WindchillAPI");
							var jsonContent = JsonSerializer.Serialize(dynamicDto);
							var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
							var response = await client.PostAsync(targetUrl, content, cancellationToken);

							var responseContent = await response.Content.ReadAsStringAsync();
							_logger.LogInformation("Endpoint yanıtı: {StatusCode} - {Content}",
								response.StatusCode, responseContent);

							if (!response.IsSuccessStatusCode)
							{
								string errorMessage = $"Endpoint {targetUrl} hatası: {response.StatusCode} - {responseContent}";
								_logger.LogWarning(errorMessage);
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
					_logger.LogWarning(warningMessage);
					endpointErrors.AppendLine(warningMessage);
					allEndpointsSucceeded = false;
				}

				// 6. İşlem sonucuna göre parçayı sil veya hata tablosunda güncelle
				if (allEndpointsSucceeded)
				{
					try
					{
						// Sent tablosuna ekle
						var wtPartAlternateSentEntity = _mapper.Map<WTPartAlternateLinkSentEntegration>(wtPartAlternateErrorEntity);
						wtPartAlternateSentEntity.LogID = 0; // ID'yi sıfırla
						wtPartAlternateSentEntity.LogDate = DateTime.Now;

						var wtPartAlternateSentEntityResp = await _genericWtpartAlternateSentRepository.AddAsync(wtPartAlternateSentEntity);

						if (wtPartAlternateSentEntityResp != null)
						{
							try
							{
								// Hata tablosundan sil
								//await _genericWtpartAlternateErrorRepository.DeleteAsync(wtPartAlternateErrorEntity, permanent: true);
								await _retryService.DeleteEntityAsync(wtPartAlternateErrorEntity, permanent: true,cancellationToken);
								request.LogMessage = "Hatalı muadil parça başarıyla işlendi ve hata tablosundan silindi.";
								_logger.LogInformation("Hatalı parça başarıyla işlendi ve silindi. Ana ParcaPartID: {AnaParcaPartID}, Muadil ParcaPartID: {MuadilParcaPartID}",
									wtPartAlternateErrorEntity.AnaParcaPartID, wtPartAlternateErrorEntity.MuadilParcaPartID);
							}
							catch (DbUpdateConcurrencyException ex)
							{
								_logger.LogWarning(ex, "Concurrency hatası oluştu, kayıt zaten silinmiş olabilir. ID: {Id}", wtPartAlternateErrorEntity.LogID);
								request.LogMessage = "Hatalı muadil parça başarıyla işlendi, ancak hata tablosundan silinirken hata oluştu.";
							}
						}
						else
						{
							_logger.LogWarning("Sent tablosuna ekleme başarısız oldu.");
							allEndpointsSucceeded = false;
							endpointErrors.AppendLine("Sent tablosuna ekleme başarısız oldu.");
						}
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Sent tablosuna ekleme işlemi sırasında hata oluştu");
						allEndpointsSucceeded = false;
						endpointErrors.AppendLine($"Sent tablosuna ekleme hatası: {ex.Message}");
					}
				}

				if (!allEndpointsSucceeded)
				{
					string errorMessage = $"Hatalı muadil parça işleminde yeni hata oluştu: {endpointErrors}";

					// Hata kaydını güncelle
					wtPartAlternateErrorEntity.LogMesaj = errorMessage;
					wtPartAlternateErrorEntity.LogDate = DateTime.Now;
					//await _genericWtpartAlternateErrorRepository.UpdateAsync(wtPartAlternateErrorEntity);
					//request.LogMessage = "Hatalı muadil parça işleminde yeni hata oluştu, kayıt hata tablosunda güncellendi.";

					// Maksimum deneme sayısını aşıp aşmadığını kontrol et
					if (_retryService.ShouldDeleteEntity(wtPartAlternateErrorEntity))
					{
						_logger.LogWarning("Maksimum deneme sayısına ulaşıldı, kayıt silinecek. ID: {Id}", wtPartAlternateErrorEntity.LogID);
						await _retryService.DeleteEntityAsync(wtPartAlternateErrorEntity, permanent: true, cancellationToken);
						request.LogMessage = "Maksimum deneme sayısına ulaşıldığı için hatalı muadil parça silindi.";
					}
					else
					{
						// Deneme sayısı aşılmadıysa güncelle
						await _retryService.UpdateEntityAsync(wtPartAlternateErrorEntity, cancellationToken);
						request.LogMessage = "Hatalı muadil parça işleminde yeni hata oluştu, kayıt hata tablosunda güncellendi.";
					}

					_logger.LogWarning("Hatalı parça işlenirken yeni hata oluştu. Ana ParcaPartID: {AnaParcaPartID}, Muadil ParcaPartID: {MuadilParcaPartID}, Hata: {Error}",
						wtPartAlternateErrorEntity.AnaParcaPartID, wtPartAlternateErrorEntity.MuadilParcaPartID, errorMessage);
				}

				// 7. Sonuç DTO'sunu oluşturuyoruz
				var responseDto = _mapper.Map<ErrorProcessWTPartAlternateLinkResponse>(wtPartAlternateErrorEntity);
				responseDto.Success = allEndpointsSucceeded;
				responseDto.Message = allEndpointsSucceeded
					? "Hatalı muadil parça başarıyla işlendi."
					: $"Hatalı muadil parça işleminde yeni hata oluştu: {endpointErrors}";

				return responseDto;
			}
			catch (Exception ex)
			{
				// Genel hata durumunda
				string errorMessage = $"İşlem sırasında beklenmeyen hata: {ex.Message}";
				_logger.LogError(ex, errorMessage);

				try
				{
					// Hata tablosundan mevcut kaydı al
					var wtPartAlternateErrorEntity = await _genericWtpartAlternateErrorRepository.GetFirstAsync();
					if (wtPartAlternateErrorEntity != null)
					{
						// Hata mesajını güncelle
						wtPartAlternateErrorEntity.LogMesaj = errorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;
						//await _genericWtpartAlternateErrorRepository.UpdateAsync(wtPartAlternateErrorEntity);
						await _retryService.UpdateEntityAsync(wtPartAlternateErrorEntity, cancellationToken);
						_logger.LogInformation("Hata kaydı güncellendi. ID: {Id}", wtPartAlternateErrorEntity.LogID);
					}
				}
				catch (Exception ex2)
				{
					_logger.LogError(ex2, "Hata kaydını güncelleme sırasında ikinci bir hata oluştu");
				}

				return new ErrorProcessWTPartAlternateLinkResponse
				{
					Success = false,
					Message = errorMessage
				};
			}
		}
	}
}

