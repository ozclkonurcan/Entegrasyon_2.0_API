using Application.Features.WindchillIntegration.WTPartAlternateLink.Commands.Process;
using Application.Interfaces.ApiService;
using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Interfaces.Generic;
using Application.Interfaces.IntegrationSettings;
using Application.Interfaces.Mail;
using Application.Pipelines.Transaction;
using Application.Pipelines.WTPartLogging.WTPartAlternateLogging;
using AutoMapper;
using Domain.Entities.WTPartModels.AlternateModels;
using Domain.Entities.WTPartModels.AlternateRemovedModels;
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
using static Application.Features.WindchillIntegration.WTPartAlternateLink.Commands.Process.ProcessWTPartAlternateLinkCommand;
using static Application.Features.WindchillIntegration.WTPartCancelled.Commands.Process.ProcessWTPartCancelledCommand;

namespace Application.Features.WindchillIntegration.WTPartAlternateLinkRemoved.Commands.Process;

public class ProcessWTPartAlternateLinkRemovedCommand : IRequest<ProcessWTPartAlternateLinkRemovedResponse>, IWTPartAlternateLoggableRequest, ITransactionalRequest
{

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
	public string LogMesaj { get; set; } = "WTPart AlternateLink Removed işlemi başlatıldı.";
	public DateTime? LogDate { get; set; } = DateTime.Now;
	public byte? EntegrasyonDurum { get; set; } = 1;

	public class ProcessWTPartAlternateLinkRemovedCommandHandler : IRequestHandler<ProcessWTPartAlternateLinkRemovedCommand, ProcessWTPartAlternateLinkRemovedResponse>
	{
		private readonly IGenericRepository<WTPartAlternateLinkRemovedEntegration> _genericWtpartAlternateRemovedRepository;
		private readonly IGenericRepository<WTPartAlternateLinkRemovedErrorEntegration> _genericWtpartAlternateRemovedErrorRepository;
		private readonly IGenericRepository<WTPartAlternateLinkRemovedLogEntegration> _genericWtpartAlternateRemovedLogRepository;
		private readonly IGenericRepository<WTPartAlternateLinkRemovedSentEntegration> _genericWtpartAlternateRemovedSentRepository;
		private readonly IStateService _stateService;
		private readonly IIntegrationSettingsService _integrationSettingsService;
		private readonly IMapper _mapper;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly Interfaces.ApiService.IApiClientService _apiClientService;
		private readonly ILogger<ProcessWTPartAlternateLinkRemovedCommandHandler> _logger;

		private readonly IMailService _mailService;


		public ProcessWTPartAlternateLinkRemovedCommandHandler(IGenericRepository<WTPartAlternateLinkRemovedEntegration> genericWtpartAlternateRemovedRepository, IGenericRepository<WTPartAlternateLinkRemovedErrorEntegration> genericWtpartAlternateRemovedErrorRepository, IGenericRepository<WTPartAlternateLinkRemovedLogEntegration> genericWtpartAlternateRemovedLogRepository, IGenericRepository<WTPartAlternateLinkRemovedSentEntegration> genericWtpartAlternateRemovedSentRepository, IStateService stateService, IIntegrationSettingsService integrationSettingsService, IMapper mapper, IHttpClientFactory httpClientFactory, IApiClientService apiClientService, ILogger<ProcessWTPartAlternateLinkRemovedCommandHandler> logger, IMailService mailService)
		{
			_genericWtpartAlternateRemovedRepository = genericWtpartAlternateRemovedRepository;
			_genericWtpartAlternateRemovedErrorRepository = genericWtpartAlternateRemovedErrorRepository;
			_genericWtpartAlternateRemovedLogRepository = genericWtpartAlternateRemovedLogRepository;
			_genericWtpartAlternateRemovedSentRepository = genericWtpartAlternateRemovedSentRepository;
			_stateService = stateService;
			_integrationSettingsService = integrationSettingsService;
			_mapper = mapper;
			_httpClientFactory = httpClientFactory;
			_apiClientService = apiClientService;
			_logger = logger;
			_mailService = mailService;
		}

		public async Task<ProcessWTPartAlternateLinkRemovedResponse> Handle(ProcessWTPartAlternateLinkRemovedCommand request, CancellationToken cancellationToken)
		{
			try
			{
				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
				{
					return new ProcessWTPartAlternateLinkRemovedResponse
					{
						Success = false,
						Message = "Muadil modülü pasif durumda."
					};
				}

				var wtPartAlternateEntity = await _genericWtpartAlternateRemovedRepository.GetFirstAsync(cancellationToken: cancellationToken);

				if (wtPartAlternateEntity == null)
				{
					return new ProcessWTPartAlternateLinkRemovedResponse
					{
						Success = false,
						Message = "Muadil durumunda veri bulunamadı."
					};
				}

				// 3. Loglama alanlarını güncelliyoruz.
			request.LogID = wtPartAlternateEntity.LogID;
                request.AnaParcaState = wtPartAlternateEntity.AnaParcaState;
                request.AnaParcaPartID = wtPartAlternateEntity.AnaParcaPartID;
                request.AnaParcaPartMasterID = wtPartAlternateEntity.AnaParcaPartMasterID;
                request.AnaParcaName = wtPartAlternateEntity.AnaParcaName;
                request.AnaParcaNumber = wtPartAlternateEntity.AnaParcaNumber;
                request.AnaParcaVersion = wtPartAlternateEntity.AnaParcaVersion;
                request.MuadilParcaState = wtPartAlternateEntity.MuadilParcaState;
                request.MuadilParcaPartID = wtPartAlternateEntity.MuadilParcaPartID;
                request.MuadilParcaMasterID = wtPartAlternateEntity.MuadilParcaMasterID;
                request.MuadilParcaName = wtPartAlternateEntity.MuadilParcaName;
                request.MuadilParcaNumber = wtPartAlternateEntity.MuadilParcaNumber;
                request.MuadilParcaVersion = wtPartAlternateEntity.MuadilParcaVersion;
                request.KulAd = wtPartAlternateEntity.KulAd ?? "unknown";
                request.LogDate = DateTime.Now;
                request.EntegrasyonDurum = 1; // İşlemde

				wtPartAlternateEntity.LogDate = DateTime.Now;
				wtPartAlternateEntity.EntegrasyonDurum = 1; // 1 = başarılı

				// 4. Rol mapping bilgisini, ProcessTagID = 6 (WTPartalternateRemoved) olarak alıyoruz. Kaldırılan muadilleri yakalamak için
				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(6);
				if (roleMapping == null || !roleMapping.IsActive)
				{
					return new ProcessWTPartAlternateLinkRemovedResponse
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
					string anaParcaUrl = $"/Windchill/servlet/odata/ProdMgmt/Parts('OR:wt.part.WTPart:{wtPartAlternateEntity.AnaParcaPartID}')";
					string muadilParcaUrl = $"/Windchill/servlet/odata/ProdMgmt/Parts('OR:wt.part.WTPart:{wtPartAlternateEntity.MuadilParcaPartID}')";

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
						windchillErrorMessage = $"Ana parça Windchill'de bulunamadı. ParcaPartID: {wtPartAlternateEntity.AnaParcaPartID}";

						var wtPartAlternateErrorEntity = _mapper.Map<WTPartAlternateLinkRemovedErrorEntegration>(wtPartAlternateEntity);
						wtPartAlternateErrorEntity.LogID = 0; // ID'yi sıfırla
						wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;

						var wtPartAlternateErrorEntityResp = await _genericWtpartAlternateRemovedErrorRepository.AddAsync(wtPartAlternateErrorEntity);
						if (wtPartAlternateErrorEntityResp != null)
						{
							try
							{
								await _genericWtpartAlternateRemovedRepository.DeleteAsync(wtPartAlternateEntity, permanent: true);
							}
							catch (DbUpdateConcurrencyException ex)
							{
								_logger.LogWarning(ex, "Concurrency hatası oluştu, kayıt zaten silinmiş olabilir. ID: {Id}", wtPartAlternateEntity.LogID);
							}
						}

						_logger.LogWarning(windchillErrorMessage);
						return new ProcessWTPartAlternateLinkRemovedResponse
						{
							Success = false,
							Message = windchillErrorMessage
						};
					}

					// Muadil parça API yanıtı kontrolü
					if (string.IsNullOrEmpty(muadilParcaJson) || muadilParcaJson == "{}" || muadilParcaJson == "null")
					{
						windchillErrorMessage = $"Muadil parça Windchill'de bulunamadı. ParcaPartID: {wtPartAlternateEntity.MuadilParcaPartID}";

						var wtPartAlternateErrorEntity = _mapper.Map<WTPartAlternateLinkRemovedErrorEntegration>(wtPartAlternateEntity);
						wtPartAlternateErrorEntity.LogID = 0; // ID'yi sıfırla
						wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;

						var wtPartAlternateErrorEntityResp = await _genericWtpartAlternateRemovedErrorRepository.AddAsync(wtPartAlternateErrorEntity);
						if (wtPartAlternateErrorEntityResp != null)
						{
							try
							{
								await _genericWtpartAlternateRemovedRepository.DeleteAsync(wtPartAlternateEntity, permanent: true);
							}
							catch (DbUpdateConcurrencyException ex)
							{
								_logger.LogWarning(ex, "Concurrency hatası oluştu, kayıt zaten silinmiş olabilir. ID: {Id}", wtPartAlternateEntity.LogID);
							}
						}

						_logger.LogWarning(windchillErrorMessage);
						return new ProcessWTPartAlternateLinkRemovedResponse
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
						windchillErrorMessage = $"Ana parça için geçersiz JSON yanıtı. ParcaPartID: {wtPartAlternateEntity.AnaParcaPartID}, Yanıt: {anaParcaJson}";

						var wtPartAlternateErrorEntity = _mapper.Map<WTPartAlternateLinkRemovedErrorEntegration>(wtPartAlternateEntity);
						wtPartAlternateErrorEntity.LogID = 0; // ID'yi sıfırla
						wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;

						var wtPartAlternateErrorEntityResp = await _genericWtpartAlternateRemovedErrorRepository.AddAsync(wtPartAlternateErrorEntity);
						if (wtPartAlternateErrorEntityResp != null)
						{
							try
							{
								await _genericWtpartAlternateRemovedRepository.DeleteAsync(wtPartAlternateEntity, permanent: true);
							}
							catch (DbUpdateConcurrencyException ex2)
							{
								_logger.LogWarning(ex2, "Concurrency hatası oluştu, kayıt zaten silinmiş olabilir. ID: {Id}", wtPartAlternateEntity.LogID);
							}
						}

						_logger.LogError(ex, windchillErrorMessage);
						return new ProcessWTPartAlternateLinkRemovedResponse
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
						windchillErrorMessage = $"Muadil parça için geçersiz JSON yanıtı. ParcaPartID: {wtPartAlternateEntity.MuadilParcaPartID}, Yanıt: {muadilParcaJson}";

						var wtPartAlternateErrorEntity = _mapper.Map<WTPartAlternateLinkRemovedErrorEntegration>(wtPartAlternateEntity);
						wtPartAlternateErrorEntity.LogID = 0; // ID'yi sıfırla
						wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;

						var wtPartAlternateErrorEntityResp = await _genericWtpartAlternateRemovedErrorRepository.AddAsync(wtPartAlternateErrorEntity);
						if (wtPartAlternateErrorEntityResp != null)
						{
							try
							{
								await _genericWtpartAlternateRemovedRepository.DeleteAsync(wtPartAlternateEntity, permanent: true);
							}
							catch (DbUpdateConcurrencyException ex2)
							{
								_logger.LogWarning(ex2, "Concurrency hatası oluştu, kayıt zaten silinmiş olabilir. ID: {Id}", wtPartAlternateEntity.LogID);
							}
						}

						_logger.LogError(ex, windchillErrorMessage);
						return new ProcessWTPartAlternateLinkRemovedResponse
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

						windchillErrorMessage = $"Ana parça Windchill API hatası: {errorMessage}. ParcaPartID: {wtPartAlternateEntity.AnaParcaPartID}";

						var wtPartAlternateErrorEntity = _mapper.Map<WTPartAlternateLinkRemovedErrorEntegration>(wtPartAlternateEntity);
						wtPartAlternateErrorEntity.LogID = 0; // ID'yi sıfırla
						wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;

						var wtPartAlternateErrorEntityResp = await _genericWtpartAlternateRemovedErrorRepository.AddAsync(wtPartAlternateErrorEntity);
						if (wtPartAlternateErrorEntityResp != null)
						{
							try
							{
								await _genericWtpartAlternateRemovedRepository.DeleteAsync(wtPartAlternateEntity, permanent: true);
							}
							catch (DbUpdateConcurrencyException ex)
							{
								_logger.LogWarning(ex, "Concurrency hatası oluştu, kayıt zaten silinmiş olabilir. ID: {Id}", wtPartAlternateEntity.LogID);
							}
						}

						_logger.LogWarning(windchillErrorMessage);
						return new ProcessWTPartAlternateLinkRemovedResponse
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

						windchillErrorMessage = $"Muadil parça Windchill API hatası: {errorMessage}. ParcaPartID: {wtPartAlternateEntity.MuadilParcaPartID}";

						var wtPartAlternateErrorEntity = _mapper.Map<WTPartAlternateLinkRemovedErrorEntegration>(wtPartAlternateEntity);
						wtPartAlternateErrorEntity.LogID = 0; // ID'yi sıfırla
						wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
						wtPartAlternateErrorEntity.LogDate = DateTime.Now;

						var wtPartAlternateErrorEntityResp = await _genericWtpartAlternateRemovedErrorRepository.AddAsync(wtPartAlternateErrorEntity);
						if (wtPartAlternateErrorEntityResp != null)
						{
							try
							{
								await _genericWtpartAlternateRemovedRepository.DeleteAsync(wtPartAlternateEntity, permanent: true);
							}
							catch (DbUpdateConcurrencyException ex)
							{
								_logger.LogWarning(ex, "Concurrency hatası oluştu, kayıt zaten silinmiş olabilir. ID: {Id}", wtPartAlternateEntity.LogID);
							}
						}

						_logger.LogWarning(windchillErrorMessage);
						return new ProcessWTPartAlternateLinkRemovedResponse
						{
							Success = false,
							Message = windchillErrorMessage
						};
					}

					// Rol ayarlarında tanımlı olan WindchillAttributes değerleriyle dinamik DTO oluşturuyoruz.
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
					dynamicDto["LogID"] = wtPartAlternateEntity.LogID;
					dynamicDto["LogDate"] = wtPartAlternateEntity.LogDate;
					dynamicDto["KulAd"] = wtPartAlternateEntity.KulAd;
					dynamicDto["LogMesaj"] = wtPartAlternateEntity.LogMesaj;
					dynamicDto["EntegrasyonDurum"] = wtPartAlternateEntity.EntegrasyonDurum;

					windchillApiSuccess = true;
				}
				catch (Exception ex)
				{
					windchillErrorMessage = string.IsNullOrEmpty(windchillErrorMessage)
						? $"Windchill API hatası: {ex.Message}. Ana ParcaPartID: {wtPartAlternateEntity.AnaParcaPartID}, Muadil ParcaPartID: {wtPartAlternateEntity.MuadilParcaPartID}"
						: windchillErrorMessage;

					var wtPartAlternateErrorEntity = _mapper.Map<WTPartAlternateLinkRemovedErrorEntegration>(wtPartAlternateEntity);
					wtPartAlternateErrorEntity.LogID = 0; // ID'yi sıfırla
					wtPartAlternateErrorEntity.LogMesaj = windchillErrorMessage;
					wtPartAlternateErrorEntity.LogDate = DateTime.Now;

					var wtPartAlternateErrorEntityResp = await _genericWtpartAlternateRemovedErrorRepository.AddAsync(wtPartAlternateErrorEntity);
					if (wtPartAlternateErrorEntityResp != null)
					{
						try
						{
							await _genericWtpartAlternateRemovedRepository.DeleteAsync(wtPartAlternateEntity, permanent: true);
						}
						catch (DbUpdateConcurrencyException ex2)
						{
							_logger.LogWarning(ex2, "Concurrency hatası oluştu, kayıt zaten silinmiş olabilir. ID: {Id}", wtPartAlternateEntity.LogID);
						}
					}

					_logger.LogError(ex, windchillErrorMessage);
					return new ProcessWTPartAlternateLinkRemovedResponse
					{
						Success = false,
						Message = windchillErrorMessage
					};
				}

				// Windchill API'den veri alınamadıysa, işlemi sonlandır
				if (!windchillApiSuccess)
				{
					return new ProcessWTPartAlternateLinkRemovedResponse
					{
						Success = false,
						Message = windchillErrorMessage
					};
				}
				#endregion

				// 5. Rol mapping'in endpoints'lerine, dinamik DTO'yu gönderiyoruz.
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

				// 6. İşlem sonucuna göre parçayı sil veya hata tablosuna aktar
				if (allEndpointsSucceeded)
				{
					try
					{
						// Sent tablosuna ekle
						var wtPartAlternateSentEntity = _mapper.Map<WTPartAlternateLinkRemovedSentEntegration>(wtPartAlternateEntity);
						wtPartAlternateSentEntity.LogID = 0; // ID'yi sıfırla
						wtPartAlternateSentEntity.LogDate = DateTime.Now;


						var wtPartAlternateSentEntityResp = await _genericWtpartAlternateRemovedSentRepository.AddAsync(wtPartAlternateSentEntity);

						if (wtPartAlternateSentEntityResp != null)
						{
							try
							{
								await _genericWtpartAlternateRemovedRepository.DeleteAsync(wtPartAlternateEntity, permanent: true);
								request.LogMessage = "Muadil ilişki kaldırma işlem başarılı şekilde tamamlandı ve parça silindi.";
								_logger.LogInformation("Parça başarıyla işlendi ve silindi. Ana ParcaPartID: {AnaParcaPartID}, Muadil ParcaPartID: {MuadilParcaPartID}",
									wtPartAlternateEntity.AnaParcaPartID, wtPartAlternateEntity.MuadilParcaPartID);
							}
							catch (DbUpdateConcurrencyException ex)
							{
								_logger.LogWarning(ex, "Concurrency hatası oluştu, kayıt zaten silinmiş olabilir. ID: {Id}", wtPartAlternateEntity.LogID);
								request.LogMessage = "Muadil işlem başarılı şekilde tamamlandı, ancak parça silinirken hata oluştu.";
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
					string errorMessage = $"Muadil işleminde hata oluştu: {endpointErrors}";
					// Burada hata tablosuna aktarma işlemi yapılabilir
					await _mailService.SendErrorMailAsync(
					"WTPartAlternateLinkRemoved",
					$"{wtPartAlternateEntity.AnaParcaNumber}-{wtPartAlternateEntity.MuadilParcaNumber}",
					$"Ana: {wtPartAlternateEntity.AnaParcaName} → Muadil: {wtPartAlternateEntity.MuadilParcaName}",
					errorMessage,
					null
					);
					request.LogMessage = "Muadil işleminde hata oluştu, parça hata tablosuna aktarıldı.";

					var wtPartAlternateErrorEntity = _mapper.Map<WTPartAlternateLinkRemovedErrorEntegration>(wtPartAlternateEntity);
					wtPartAlternateErrorEntity.LogID = 0; // ID'yi sıfırla
					wtPartAlternateErrorEntity.LogMesaj = errorMessage;
					wtPartAlternateErrorEntity.LogDate = DateTime.Now;

					var wtPartAlternateErrorEntityResp = await _genericWtpartAlternateRemovedErrorRepository.AddAsync(wtPartAlternateErrorEntity);

					if (wtPartAlternateErrorEntityResp != null)
					{
						try
						{
							await _genericWtpartAlternateRemovedRepository.DeleteAsync(wtPartAlternateEntity, permanent: true);
						}
						catch (DbUpdateConcurrencyException ex)
						{
							_logger.LogWarning(ex, "Concurrency hatası oluştu, kayıt zaten silinmiş olabilir. ID: {Id}", wtPartAlternateEntity.LogID);
						}
					}

					_logger.LogWarning("Parça hata tablosuna aktarıldı. Ana ParcaPartID: {AnaParcaPartID}, Muadil ParcaPartID: {MuadilParcaPartID}, Hata: {Error}",
						wtPartAlternateEntity.AnaParcaPartID, wtPartAlternateEntity.MuadilParcaPartID, errorMessage);
				}

				// 7. Sonuç DTO'sunu oluşturuyoruz.
				var responseDto = _mapper.Map<ProcessWTPartAlternateLinkRemovedResponse>(wtPartAlternateEntity);
				responseDto.Success = allEndpointsSucceeded;
				responseDto.Message = allEndpointsSucceeded
					? "Muadil işlem başarılı şekilde tamamlandı."
					: $"Muadil işleminde hata oluştu: {endpointErrors}";

				return responseDto;
			}
			catch (Exception ex)
			{
				// Genel hata durumunda
				string errorMessage = $"İşlem sırasında beklenmeyen hata: {ex.Message}";
				await _mailService.SendErrorMailAsync(
				"WTPartAlternateLinkRemoved",
				$"{request.AnaParcaNumber}-{request.MuadilParcaNumber}",
				$"Ana: {request.AnaParcaName} → Muadil: {request.MuadilParcaName}",
				errorMessage,
				null
				);

				try
				{
					var wtPartAlternateErrorEntity = _mapper.Map<WTPartAlternateLinkRemovedErrorEntegration>(request);
					wtPartAlternateErrorEntity.LogID = 0; // ID'yi sıfırla
					wtPartAlternateErrorEntity.LogMesaj = errorMessage;
					wtPartAlternateErrorEntity.LogDate = DateTime.Now;

					var wtPartAlternateErrorEntityResp = await _genericWtpartAlternateRemovedErrorRepository.AddAsync(wtPartAlternateErrorEntity);

					// Ana tablodan silme işlemi
					var wtPartAlternateEntity = await _genericWtpartAlternateRemovedRepository.GetFirstAsync();
					if (wtPartAlternateEntity != null && wtPartAlternateErrorEntityResp != null)
					{
						try
						{
							await _genericWtpartAlternateRemovedRepository.DeleteAsync(wtPartAlternateEntity, permanent: true);
						}
						catch (DbUpdateConcurrencyException ex2)
						{
							_logger.LogWarning(ex2, "Concurrency hatası oluştu, kayıt zaten silinmiş olabilir. ID: {Id}", wtPartAlternateEntity.LogID);
						}
					}
				}
				catch (Exception ex2)
				{
					_logger.LogError(ex2, "Error tablosuna ekleme veya ana tablodan silme işlemi sırasında hata oluştu");
				}

				_logger.LogError(ex, errorMessage);

				return new ProcessWTPartAlternateLinkRemovedResponse
				{
					Success = false,
					Message = errorMessage
				};
			}
		}

	}
}
