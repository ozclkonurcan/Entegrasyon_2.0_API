


#region Test Kod
using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Interfaces.Generic;
using Application.Interfaces.IntegrationSettings;
using Application.Interfaces.Mail;
using Application.Pipelines.MailNotification;
using Application.Pipelines.Transaction;
using Application.Pipelines.WTPartLogging;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.IntegrationSettings;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartCancelled.Commands.Process
{
	public class ProcessWTPartCancelledCommand : IRequest<ProcessWTPartCancelledResponse>, IWTPartLoggableRequest, ITransactionalRequest
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


		public ProcessWTPartCancelledCommand()
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

		public class ProcessWTPartCancelledCommandHandler : IRequestHandler<ProcessWTPartCancelledCommand, ProcessWTPartCancelledResponse>
		{
			private readonly IWTPartService<WTPart> _wTPartService;
			private readonly IGenericRepository<WTPart> _genericWtpartRepository;
			private readonly IGenericRepository<WTPartSentDatas> _genericWtpartSentRepository;

			private readonly IStateService _stateService;
			private readonly IIntegrationSettingsService _integrationSettingsService;
			private readonly IMapper _mapper;
			private readonly IHttpClientFactory _httpClientFactory;
			private readonly Interfaces.ApiService.IApiClientService _apiClientService;
			private readonly ILogger<ProcessWTPartCancelledCommandHandler> _logger;

			private readonly IMailService _mailService;


			public ProcessWTPartCancelledCommandHandler(
				IWTPartService<WTPart> wTPartService,
				IStateService stateService,
				IMapper mapper,
				IIntegrationSettingsService integrationSettingsService,
				IHttpClientFactory httpClientFactory,
				Interfaces.ApiService.IApiClientService apiClientService,
				ILogger<ProcessWTPartCancelledCommandHandler> logger,
				IGenericRepository<WTPartSentDatas> genericWtpartSentRepository,
				IGenericRepository<WTPart> genericWtpartRepository,
				IMailService mailService)
			{
				_wTPartService = wTPartService;
				_stateService = stateService;
				_mapper = mapper;
				_integrationSettingsService = integrationSettingsService;
				_httpClientFactory = httpClientFactory;
				_apiClientService = apiClientService;
				_logger = logger;
				_genericWtpartSentRepository = genericWtpartSentRepository;
				_genericWtpartRepository = genericWtpartRepository;
				_mailService = mailService;
			}

			public async Task<ProcessWTPartCancelledResponse> Handle(ProcessWTPartCancelledCommand request, CancellationToken cancellationToken)
			{
				try
				{
					// 1. Modül ayarlarını kontrol ediyoruz.
					var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
					if (moduleSettings == null || moduleSettings.SettingsValue == 0)
					{
						return new ProcessWTPartCancelledResponse
						{
							Success = false,
							Message = "WTPartCancelled modülü pasif durumda."
						};
					}

					// 2. CANCELLED durumundaki parçayı veritabanından çekiyoruz.
					var wtPartEntity = await _stateService.CANCELLED(cancellationToken);
					if (wtPartEntity == null)
					{
						return new ProcessWTPartCancelledResponse
						{
							Success = false,
							Message = "Cancelled durumunda veri bulunamadı."
						};
					}

					// 3. Loglama alanlarını güncelliyoruz.
					request.ParcaPartID = wtPartEntity.ParcaPartID.ToString();
					request.ParcaPartMasterID = wtPartEntity.ParcaPartMasterID.ToString();
					request.ParcaName = wtPartEntity.ParcaName;
					request.ParcaNumber = wtPartEntity.ParcaNumber;
					request.ParcaVersion = wtPartEntity.ParcaVersion;

					wtPartEntity.LogDate = DateTime.Now;
					wtPartEntity.EntegrasyonDurum = 1; // 1 = başarılı

					// 4. Rol mapping bilgisini, ProcessTagID = 2 (örneğin, Cancelled rolü için) çekiyoruz.
					var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(2);
					if (roleMapping == null || !roleMapping.IsActive)
					{
						return new ProcessWTPartCancelledResponse
						{
							Success = false,
							Message = "WTPartCancelled rol ayarı bulunamadı veya pasif durumda."
						};
					}

					#region Dinamik Attribute Gönderimi
					IDictionary<string, object> dynamicDto = new ExpandoObject();
					bool windchillApiSuccess = false;
					string windchillErrorMessage = string.Empty;

					try
					{
						// Windchill API'den, ilgili parçanın detaylarını çekmek için URL oluşturuyoruz.
						string windchillUrl = $"/Windchill/servlet/odata/ProdMgmt/Parts('OR:wt.part.WTPart:{wtPartEntity.ParcaPartID}')";
						_logger.LogInformation("Windchill API isteği: {Url}", windchillUrl);

						string windchillJson = await _apiClientService.GetAsync<string>(windchillUrl);
						_logger.LogInformation("Windchill API yanıtı: {Response}", windchillJson);

						// API yanıtı boş veya null ise, parça bulunamadı demektir
						if (string.IsNullOrEmpty(windchillJson) || windchillJson == "{}" || windchillJson == "null")
						{
							windchillErrorMessage = $"Parça Windchill'de bulunamadı. ParcaPartID: {wtPartEntity.ParcaPartID}";
							_logger.LogWarning(windchillErrorMessage);

							// Parça bulunamadığında, parçayı hata tablosuna aktar
							await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, windchillErrorMessage);

							return new ProcessWTPartCancelledResponse
							{
								Success = false,
								Message = windchillErrorMessage
							};
						}

						// JSON parse etmeye çalış
						JsonDocument jsonDoc;
						try
						{
							jsonDoc = JsonDocument.Parse(windchillJson);
						}
						catch (JsonException ex)
						{
							windchillErrorMessage = $"Geçersiz JSON yanıtı. ParcaPartID: {wtPartEntity.ParcaPartID}, Yanıt: {windchillJson}";
							_logger.LogError(ex, windchillErrorMessage);

							// Geçersiz JSON durumunda, parçayı hata tablosuna aktar
							await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, windchillErrorMessage);

							return new ProcessWTPartCancelledResponse
							{
								Success = false,
								Message = windchillErrorMessage
							};
						}

						var rootElement = jsonDoc.RootElement;

						// JSON'da "error" alanı var mı kontrol et (OData hata formatı)
						if (rootElement.TryGetProperty("error", out JsonElement errorElement))
						{
							string errorMessage = "Bilinmeyen hata";
							if (errorElement.TryGetProperty("message", out JsonElement messageElement))
							{
								errorMessage = messageElement.GetString() ?? errorMessage;
							}

							windchillErrorMessage = $"Windchill API hatası: {errorMessage}. ParcaPartID: {wtPartEntity.ParcaPartID}";
							_logger.LogWarning(windchillErrorMessage);

							// API hata döndürdüğünde, parçayı hata tablosuna aktar
							await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, windchillErrorMessage);

							return new ProcessWTPartCancelledResponse
							{
								Success = false,
								Message = windchillErrorMessage
							};
						}

						// Rol ayarlarında tanımlı olan WindchillAttributes değerleriyle dinamik DTO oluşturuyoruz.
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

						windchillApiSuccess = true;
					}
					catch (Exception ex)
					{
						windchillErrorMessage = string.IsNullOrEmpty(windchillErrorMessage)
							? $"Windchill API hatası: {ex.Message}. ParcaPartID: {wtPartEntity.ParcaPartID}"
							: windchillErrorMessage;

						_logger.LogError(ex, windchillErrorMessage);

						// Windchill API hatası durumunda, parçayı hata tablosuna aktar
						await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, windchillErrorMessage);

						return new ProcessWTPartCancelledResponse
						{
							Success = false,
							Message = windchillErrorMessage
						};
					}

					// Windchill API'den veri alınamadıysa, işlemi sonlandır
					if (!windchillApiSuccess)
					{
						return new ProcessWTPartCancelledResponse
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

					// 7. İşlem sonucuna göre parçayı sil veya hata tablosunda güncelle
					if (allEndpointsSucceeded)
					{
						// Başarılı ise, önce Sent tablosuna ekle
						var wtPartSentData = new WTPartSentDatas
						{
							ParcaPartID = wtPartEntity.ParcaPartID,
							ParcaPartMasterID = wtPartEntity.ParcaPartMasterID,
							ParcaName = wtPartEntity.ParcaName,
							ParcaNumber = wtPartEntity.ParcaNumber,
							ParcaVersion = wtPartEntity.ParcaVersion,
							ParcaState = wtPartEntity.ParcaState,
							EntegrasyonDurum = 1, // Başarılı
							KulAd = wtPartEntity.KulAd ?? "unknown",
							LogMesaj = "Cancelled işlem başarılı şekilde tamamlandı.",
							LogDate = DateTime.Now,
							ActionType = "ProcessWTPartCancelled",
							ActionDate = DateTime.Now
						};

						// Sent tablosuna ekle
						await _genericWtpartSentRepository.AddAsync(wtPartSentData);

						// Sonra parçayı sil
						//await _wTPartService.DeleteCancelledPartAsync(wtPartEntity, permanent: false);
						await _genericWtpartRepository.DeleteAsync(wtPartEntity, permanent: true);


						request.LogMessage = "Cancelled işlem başarılı şekilde tamamlandı ve parça silindi.";
					}
					else
					{
						string errorMessage = $"Cancelled işleminde hata oluştu: {endpointErrors}";
						await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, errorMessage);
						await _mailService.SendErrorMailAsync("WTPartCancelled", request.ParcaNumber, request.ParcaName, errorMessage, null);
						request.LogMessage = "Cancelled işleminde hata oluştu, parça hata tablosuna aktarıldı.";
						_logger.LogWarning("Parça hata tablosuna aktarıldı. ParcaPartID: {ParcaPartID}, Hata: {Error}",
							wtPartEntity.ParcaPartID, errorMessage);
					}

					

					// 7. Sonuç DTO'sunu oluşturuyoruz.
					var responseDto = _mapper.Map<ProcessWTPartCancelledResponse>(wtPartEntity);
					responseDto.Success = allEndpointsSucceeded;
					responseDto.Message = allEndpointsSucceeded
						? "Cancelled işlem başarılı şekilde tamamlandı."
						: $"Cancelled işleminde hata oluştu: {endpointErrors}";

					return responseDto;
				}
				catch (Exception ex)
				{
					// Genel hata durumunda
					await _mailService.SendErrorMailAsync("WTPartCancelled", request.ParcaNumber, request.ParcaName, ex.Message, null);
					string errorMessage = $"İşlem sırasında beklenmeyen hata: {ex.Message}";
					_logger.LogError(ex, errorMessage);

					// Eğer wtPartEntity oluşturulmuşsa, hata tablosuna aktar
					if (request.ParcaPartID != string.Empty)
					{
						try
						{
							WTPart wtPartEntity = new WTPart
							{
								ParcaPartID = !string.IsNullOrEmpty(request.ParcaPartID) ? long.Parse(request.ParcaPartID) : 0,
								ParcaPartMasterID = !string.IsNullOrEmpty(request.ParcaPartMasterID) ? long.Parse(request.ParcaPartMasterID) : 0,
								ParcaName = request.ParcaName ?? string.Empty,
								ParcaNumber = request.ParcaNumber ?? string.Empty,
								ParcaVersion = request.ParcaVersion ?? string.Empty,
								ParcaState = request.ParcaState ?? "CANCELLED",
								EntegrasyonDurum = 2, // Hata durumu
								LogMesaj = errorMessage,
								LogDate = DateTime.Now
							};

							// Hata tablosuna aktar
							await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, errorMessage);
							if (wtPartEntity != null)
							{
								await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, errorMessage);
								_logger.LogInformation("Parça hata tablosuna aktarıldı. ParcaPartID: {ParcaPartID}", request.ParcaPartID);
							}
						}
						catch (Exception innerEx)
						{
							_logger.LogError(innerEx, "Parça hata tablosuna aktarılırken hata oluştu. ParcaPartID: {ParcaPartID}",
								request.ParcaPartID);
						}
					}

					return new ProcessWTPartCancelledResponse
					{
						Success = false,
						Message = errorMessage
					};
				}
			}
		}
	}
}
#endregion


#region Orjinal Kod


//using Application.Interfaces.EntegrasyonModulu.WTPartServices;
//using Application.Interfaces.IntegrationSettings;
//using Application.Pipelines.WTPartLogging;
//using AutoMapper;
//using MediatR;
//using System;
//using System.Dynamic;
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;
//using Domain.Entities.IntegrationSettings;
//using Application.Pipelines.Transaction;
//using Domain.Entities;

//namespace Application.Features.WindchillIntegration.WTPartCancelled.Commands.Process
//{
//	public class ProcessWTPartCancelledCommand : IRequest<ProcessWTPartCancelledResponse>, IWTPartLoggableRequest,ITransactionalRequest
//	{
//		// Loglama için gerekli alanlar
//		public string LogMessage { get; set; }
//		public string ParcaState { get; set; }
//		public string ParcaPartID { get; set; }
//		public string ParcaPartMasterID { get; set; }
//		public string ParcaName { get; set; }
//		public string ParcaNumber { get; set; }
//		public string ParcaVersion { get; set; }
//		public string ActionType { get; set; }
//		public string EntegrasyonDurum { get; set; }

//		public ProcessWTPartCancelledCommand()
//		{
//			LogMessage = "WTPart Cancelled işlemi başlatıldı.";
//			ParcaState = "CANCELLED";
//			ParcaPartID = string.Empty;
//			ParcaPartMasterID = string.Empty;
//			ParcaName = string.Empty;
//			ParcaNumber = string.Empty;
//			ParcaVersion = string.Empty;
//			ActionType = "ProcessWTPartCancelled";
//			EntegrasyonDurum = "Parca islemde";
//		}

//		public class ProcessWTPartCancelledCommandHandler : IRequestHandler<ProcessWTPartCancelledCommand, ProcessWTPartCancelledResponse>
//		{
//			private readonly IWTPartService<WTPart> _wTPartService;
//			private readonly IStateService _stateService;
//			private readonly IIntegrationSettingsService _integrationSettingsService;
//			private readonly IMapper _mapper;
//			private readonly IHttpClientFactory _httpClientFactory;
//			private readonly Interfaces.ApiService.IApiClientService _apiClientService;

//			public ProcessWTPartCancelledCommandHandler(
//				IWTPartService<WTPart> wTPartService,
//				IStateService stateService,
//				IMapper mapper,
//				IIntegrationSettingsService integrationSettingsService,
//				IHttpClientFactory httpClientFactory,
//				Interfaces.ApiService.IApiClientService apiClientService)
//			{
//				_wTPartService = wTPartService;
//				_stateService = stateService;
//				_mapper = mapper;
//				_integrationSettingsService = integrationSettingsService;
//				_httpClientFactory = httpClientFactory;
//				_apiClientService = apiClientService;
//			}

//			public async Task<ProcessWTPartCancelledResponse> Handle(ProcessWTPartCancelledCommand request, CancellationToken cancellationToken)
//			{
//				// 1. Modül ayarlarını kontrol ediyoruz.
//				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
//				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
//				{
//					return new ProcessWTPartCancelledResponse
//					{
//						Success = false,
//						Message = "WTPartCancelled modülü pasif durumda."
//					};
//				}

//				// 2. CANCELLED durumundaki parçayı veritabanından çekiyoruz.
//				var wtPartEntity = await _stateService.CANCELLED(cancellationToken);
//				if (wtPartEntity == null)
//				{
//					return new ProcessWTPartCancelledResponse
//					{
//						Success = false,
//						Message = "Cancelled durumunda veri bulunamadı."
//					};
//				}

//				// 3. Loglama alanlarını güncelliyoruz.
//				request.ParcaPartID = wtPartEntity.ParcaPartID.ToString();
//				request.ParcaPartMasterID = wtPartEntity.ParcaPartMasterID.ToString();
//				request.ParcaName = wtPartEntity.ParcaName;
//				request.ParcaNumber = wtPartEntity.ParcaNumber;
//				request.ParcaVersion = wtPartEntity.ParcaVersion;

//				wtPartEntity.LogDate = DateTime.Now;
//				wtPartEntity.EntegrasyonDurum = 1; // 1 = başarılı

//				// 4. Rol mapping bilgisini, ProcessTagID = 2 (örneğin, Cancelled rolü için) çekiyoruz.
//				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(2);
//				if (roleMapping == null || !roleMapping.IsActive)
//				{
//					return new ProcessWTPartCancelledResponse
//					{
//						Success = false,
//						Message = "WTPartCancelled rol ayarı bulunamadı veya pasif durumda."
//					};
//				}

//				#region Dinamik Attribute Gönderimi

//				// Windchill API'den, ilgili parçanın detaylarını çekmek için URL oluşturuyoruz.
//				string windchillUrl = $"/Windchill/servlet/odata/ProdMgmt/Parts('OR:wt.part.WTPart:{wtPartEntity.ParcaPartID}')";
//				string windchillJson = await _apiClientService.GetAsync<string>(windchillUrl);
//				using var jsonDoc = JsonDocument.Parse(windchillJson);
//				var rootElement = jsonDoc.RootElement;

//				// Rol ayarlarında tanımlı olan WindchillAttributes değerleriyle dinamik DTO oluşturuyoruz.
//				IDictionary<string, object> dynamicDto = new ExpandoObject();
//				if (roleMapping.WindchillAttributes != null && roleMapping.WindchillAttributes.Any())
//				{
//					foreach (var attribute in roleMapping.WindchillAttributes)
//					{
//						if (rootElement.TryGetProperty(attribute.AttributeName, out JsonElement jsonValue))
//						{
//							if (jsonValue.ValueKind == JsonValueKind.String)
//							{
//								dynamicDto[attribute.AttributeName] = jsonValue.GetString();
//							}
//							else if (jsonValue.ValueKind == JsonValueKind.Object || jsonValue.ValueKind == JsonValueKind.Array)
//							{
//								dynamicDto[attribute.AttributeName] = JsonSerializer.Deserialize<object>(jsonValue.GetRawText());
//							}
//							else
//							{
//								dynamicDto[attribute.AttributeName] = jsonValue.ToString();
//							}
//						}
//						else
//						{
//							dynamicDto[attribute.AttributeName] = null;
//						}
//					}
//				}
//				#endregion

//				// 5. Rol mapping'in endpoints'lerine, dinamik DTO'yu gönderiyoruz.
//				bool allEndpointsSucceeded = true;
//				if (roleMapping.Endpoints != null)
//				{
//					foreach (var endpoint in roleMapping.Endpoints)
//					{
//						var targetUrl = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
//						try
//						{
//							var client = _httpClientFactory.CreateClient();
//							var jsonContent = JsonSerializer.Serialize(dynamicDto);
//							var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
//							var response = await client.PostAsync(targetUrl, content, cancellationToken);
//							if (!response.IsSuccessStatusCode)
//							{
//								allEndpointsSucceeded = false;
//							}
//						}
//						catch (Exception ex)
//						{
//							allEndpointsSucceeded = false;
//							// Hata loglama yapılabilir: ex.Message
//						}
//					}
//				}

//				// 6. Eğer tüm endpoint gönderimleri başarılı ise, parçayı siliyoruz.
//				if (allEndpointsSucceeded)
//				{
//					await _wTPartService.DeleteCancelledPartAsync(wtPartEntity, permanent: false);
//					request.LogMessage = "Cancelled işlem başarılı şekilde tamamlandı ve parça silindi.";
//				}
//				else
//				{
//					string errorMessage = "Cancelled işleminde hata oluştu, bir veya daha fazla endpoint'e gönderim başarısız.";
//					await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, errorMessage);

//					request.LogMessage = "Cancelled işleminde hata oluştu, parça hata tablosuna aktarıldı.";
//				}

//				// 7. Sonuç DTO'sunu oluşturuyoruz.
//				var responseDto = _mapper.Map<ProcessWTPartCancelledResponse>(wtPartEntity);
//				responseDto.Success = allEndpointsSucceeded;
//				responseDto.Message = allEndpointsSucceeded
//					? "Cancelled işlem başarılı şekilde tamamlandı."
//					: "Cancelled işleminde hata oluştu, parça gönderilemedi.";
//				return responseDto;
//			}
//		}
//	}
//}


#endregion

//using Application.Interfaces.EntegrasyonModulu.WTPartServices;
//using Application.Interfaces.IntegrationSettings;
//using Application.Pipelines.WTPartLogging;
//using AutoMapper;
//using MediatR;
//using System;
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;
//using Domain.Entities.IntegrationSettings; // Rol mapping, WTPart, vb.

//namespace Application.Features.WindchillIntegration.WTPartCancelled.Commands
//{
//	public class ProcessWTPartCancelledCommand : IRequest<ProcessWTPartCancelledResponse>, IWTPartLoggableRequest
//	{
//		// Loglama için gerekli alanlar
//		public string LogMessage { get; set; }
//		public string ParcaState { get; set; }
//		public string ParcaPartID { get; set; }
//		public string ParcaPartMasterID { get; set; }
//		public string ParcaName { get; set; }
//		public string ParcaNumber { get; set; }
//		public string ParcaVersion { get; set; }
//		public string ActionType { get; set; }
//		public string EntegrasyonDurum { get; set; }

//		public ProcessWTPartCancelledCommand()
//		{
//			LogMessage = "WTPart Cancelled işlemi başlatıldı.";
//			ParcaState = "CANCELLED";
//			ParcaPartID = string.Empty;
//			ParcaPartMasterID = string.Empty;
//			ParcaName = string.Empty;
//			ParcaNumber = string.Empty;
//			ParcaVersion = string.Empty;
//			ActionType = "ProcessWTPartCancelled";
//			EntegrasyonDurum = "Parca islemde";
//		}

//		public class ProcessWTPartCancelledCommandHandler : IRequestHandler<ProcessWTPartCancelledCommand, ProcessWTPartCancelledResponse>
//		{
//			private readonly IWTPartService _wTPartService;
//			private readonly IStateService _stateService;
//			private readonly IIntegrationSettingsService _integrationSettingsService;
//			private readonly IMapper _mapper;
//			private readonly IHttpClientFactory _httpClientFactory;

//			public ProcessWTPartCancelledCommandHandler(
//				IWTPartService wTPartService,
//				IStateService stateService,
//				IMapper mapper,
//				IIntegrationSettingsService integrationSettingsService,
//				IHttpClientFactory httpClientFactory)
//			{
//				_wTPartService = wTPartService;
//				_stateService = stateService;
//				_mapper = mapper;
//				_integrationSettingsService = integrationSettingsService;
//				_httpClientFactory = httpClientFactory;
//			}

//			public async Task<ProcessWTPartCancelledResponse> Handle(ProcessWTPartCancelledCommand request, CancellationToken cancellationToken)
//			{
//				// 1. Modül ayarlarını kontrol ediyoruz (örneğin, aynı "IntegrationModule" kullanılıyor).
//				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
//				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
//				{
//					return new ProcessWTPartCancelledResponse
//					{
//						Success = false,
//						Message = "WTPartCancelled modülü pasif durumda."
//					};
//				}

//				// 2. CANCELLED durumundaki parçayı veritabanından çekiyoruz.
//				var wtPartEntity = await _stateService.CANCELLED(cancellationToken);
//				if (wtPartEntity == null)
//				{
//					return new ProcessWTPartCancelledResponse
//					{
//						Success = false,
//						Message = "Cancelled durumunda veri bulunamadı."
//					};
//				}

//				// 3. Loglama alanlarını güncelliyoruz.
//				request.ParcaPartID = wtPartEntity.ParcaPartID.ToString();
//				request.ParcaPartMasterID = wtPartEntity.ParcaPartMasterID.ToString();
//				request.ParcaName = wtPartEntity.ParcaName;
//				request.ParcaNumber = wtPartEntity.ParcaNumber;
//				request.ParcaVersion = wtPartEntity.ParcaVersion;
//				// Örneğin; LogDate ve EntegrasyonDurum gibi alanları güncelliyoruz.
//				wtPartEntity.LogDate = DateTime.Now;
//				wtPartEntity.EntegrasyonDurum = 1; // Örneğin, 1 = başarılı

//				// 4. Rol mapping bilgisini, ProcessTagID = 2 (örneğin, Cancelled rolü için) çekiyoruz.
//				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(2);
//				if (roleMapping == null || !roleMapping.IsActive)
//				{
//					return new ProcessWTPartCancelledResponse
//					{
//						Success = false,
//						Message = "WTPartCancelled rol ayarı bulunamadı veya pasif durumda."
//					};
//				}

//				// 5. Rol mapping'in endpoints'lerine, parça verisini gönderiyoruz.
//				bool allEndpointsSucceeded = true;
//				if (roleMapping.Endpoints != null)
//				{
//					foreach (var endpoint in roleMapping.Endpoints)
//					{
//						var targetUrl = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
//						try
//						{
//							var client = _httpClientFactory.CreateClient();
//							var jsonContent = JsonSerializer.Serialize(wtPartEntity);
//							var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
//							var response = await client.PostAsync(targetUrl, content, cancellationToken);
//							if (!response.IsSuccessStatusCode)
//							{
//								allEndpointsSucceeded = false;
//							}
//						}
//						catch (Exception ex)
//						{
//							allEndpointsSucceeded = false;
//							// Loglama: ex.Message
//						}
//					}
//				}

//				// 6. Eğer tüm endpoint gönderimleri başarılı ise, parçayı siliyoruz.
//				if (allEndpointsSucceeded)
//				{
//					await _wTPartService.DeleteCancelledPartAsync(wtPartEntity, permanent: false);
//					request.LogMessage = "Cancelled işlem başarılı şekilde tamamlandı ve parça silindi.";
//				}
//				else
//				{
//					request.LogMessage = "Cancelled işleminde hata oluştu, parça silinmedi.";
//				}

//				// 7. Sonuç DTO'sunu oluşturuyoruz.
//				var responseDto = _mapper.Map<ProcessWTPartCancelledResponse>(wtPartEntity);
//				responseDto.Success = allEndpointsSucceeded;
//				responseDto.Message = allEndpointsSucceeded
//					? "Cancelled işlem başarılı şekilde tamamlandı."
//					: "Cancelled işleminde hata oluştu, parça gönderilemedi.";
//				return responseDto;
//			}
//		}
//	}
//}
