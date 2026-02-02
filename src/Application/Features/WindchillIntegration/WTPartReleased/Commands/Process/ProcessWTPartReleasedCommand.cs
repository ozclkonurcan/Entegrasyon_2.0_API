
#region Test Kod
using Application.Features.MailService.Commands.SendMail;
using Application.Interfaces.ApiService;
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

namespace Application.Features.WindchillIntegration.WTPartReleased.Commands.Process
{

    //Bazı parçalarda hata var şöyle bir hata artık parçada ne farklı ise o parça gönderilmiyor bütün parçalarda değil sadece bazı parçalarda hata var ya sql de
	//tetik alınan bir parça triggera bakılacak yada bizi kod içinde bir kontrol ile falan bunu halledicez yada windchil üzerinde bazı parçaların yapısı farklı
	//bunların kontrolününde yapılması lazım

    public class ProcessWTPartReleasedCommand : IRequest<ProcessWTPartReleasedResponse>, IWTPartLoggableRequest, ITransactionalRequest
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


		

		public ProcessWTPartReleasedCommand()
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

		public class ProcessWTPartReleasedCommandHandler : IRequestHandler<ProcessWTPartReleasedCommand, ProcessWTPartReleasedResponse>
		{
			private readonly IWTPartService<WTPart> _wTPartService;
			private readonly IGenericRepository<WTPart> _genericWtpartRepository;
			private readonly IGenericRepository<WTPartSentDatas> _genericWtpartSentRepository;

			private readonly IApiClientService _apiClientService;
			private readonly IStateService _stateService;
			private readonly IIntegrationSettingsService _integrationSettingsService;
			private readonly IMapper _mapper;
			private readonly IHttpClientFactory _httpClientFactory;
			private readonly ILogger<ProcessWTPartReleasedCommandHandler> _logger;

			private readonly IMediator _mediator;

			private readonly IMailService _mailService;

			public ProcessWTPartReleasedCommandHandler(
				IWTPartService<WTPart> wTPartService,
				IStateService stateService,
				IMapper mapper,
				IIntegrationSettingsService integrationSettingsService,
				IHttpClientFactory httpClientFactory,
				IApiClientService apiClientService,
				ILogger<ProcessWTPartReleasedCommandHandler> logger,
				IGenericRepository<WTPartSentDatas> genericWtpartSentRepository,
				IGenericRepository<WTPart> genericWtpartRepository,
				IMediator mediator,
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
				_mediator = mediator;
				_mailService = mailService;
			}

			public async Task<ProcessWTPartReleasedResponse> Handle(ProcessWTPartReleasedCommand request, CancellationToken cancellationToken)
			{
				try
				{
					// 1. Modül ayarlarını kontrol ediyoruz.
					var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
					if (moduleSettings == null || moduleSettings.SettingsValue == 0)
					{
						return new ProcessWTPartReleasedResponse
						{
							Success = false,
							Message = "WTPartReleased modülü pasif durumda."
						};
					}

					// 2. İşlenecek parçayı çekiyoruz.
					var wtPartEntity = await _stateService.RELEASED(cancellationToken);
					if (wtPartEntity == null)
					{
						return new ProcessWTPartReleasedResponse
						{
							Success = false,
							Message = "Released durumunda veri bulunamadı."
						};
					}

					// 3. Loglama alanlarını güncelliyoruz.
					request.ParcaPartID = wtPartEntity.ParcaPartID.ToString();
					request.ParcaPartMasterID = wtPartEntity.ParcaPartMasterID.ToString();
					request.ParcaName = wtPartEntity.ParcaName;
					request.ParcaNumber = wtPartEntity.ParcaNumber;
					request.ParcaVersion = wtPartEntity.ParcaVersion;

					// 3.1. Gerekli alanları dolduruyoruz: örneğin LogDate ve EntegrasyonDurum
					wtPartEntity.LogDate = DateTime.Now;
					wtPartEntity.EntegrasyonDurum = 1; // 1 = başarılı

					// 4. Rol mapping bilgisini, ProcessTagID = 1 (WTPartReleased) olarak alıyoruz.
					var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(1);
					if (roleMapping == null || !roleMapping.IsActive)
					{
						return new ProcessWTPartReleasedResponse
						{
							Success = false,
							Message = "WTPartReleased rol ayarı bulunamadı veya pasif durumda."
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

							return new ProcessWTPartReleasedResponse
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

							return new ProcessWTPartReleasedResponse
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

							return new ProcessWTPartReleasedResponse
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

						return new ProcessWTPartReleasedResponse
						{
							Success = false,
							Message = windchillErrorMessage
						};
					}

					// Windchill API'den veri alınamadıysa, işlemi sonlandır
					if (!windchillApiSuccess)
					{
						return new ProcessWTPartReleasedResponse
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
					#region Sentdata guncel

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
							KulAd = wtPartEntity.KulAd ?? "unknown",
							ParcaState = wtPartEntity.ParcaState,
							EntegrasyonDurum = 1, // Başarılı
							LogMesaj = "Released işlem başarılı şekilde tamamlandı.",
							LogDate = DateTime.Now,
							ActionType = "ProcessWTPartReleased",
							ActionDate = DateTime.Now
						};

						// Sent tablosuna ekle
						await _genericWtpartSentRepository.AddAsync(wtPartSentData);

						// Sonra parçayı sil
						await _genericWtpartRepository.DeleteAsync(wtPartEntity, permanent: true);


						request.LogMessage = "Released işlem başarılı şekilde tamamlandı ve parça silindi.";


					}
					else
					{
						string errorMessage = $"Released işleminde hata oluştu: {endpointErrors}";
						await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, errorMessage);
						await _mailService.SendErrorMailAsync("WTPartReleased", request.ParcaNumber, request.ParcaName, errorMessage, null);
						request.LogMessage = "Released işleminde hata oluştu, parça hata tablosuna aktarıldı.";
						_logger.LogWarning("Parça hata tablosuna aktarıldı. ParcaPartID: {ParcaPartID}, Hata: {Error}",
							wtPartEntity.ParcaPartID, errorMessage);
					}


			
					#endregion

					// 7. Sonuç DTO'sunu oluşturuyoruz.
					var responseDto = _mapper.Map<ProcessWTPartReleasedResponse>(wtPartEntity);
					responseDto.Success = allEndpointsSucceeded;
					responseDto.Message = allEndpointsSucceeded
						? "Released işlem başarılı şekilde tamamlandı."
						: $"Released işleminde hata oluştu: {endpointErrors}";

					return responseDto;
				}
				catch (Exception ex)
				{
					// Genel hata durumunda
					await _mailService.SendErrorMailAsync("WTPartReleased", request.ParcaNumber, request.ParcaName, ex.Message, null);
					string errorMessage = $"İşlem sırasında beklenmeyen hata: {ex.Message}";
					_logger.LogError(ex, errorMessage);

					// Eğer request'te parça bilgileri varsa, yeni bir WTPart nesnesi oluşturup hata tablosuna aktar
					if (request.ParcaPartID != string.Empty)
					{
						try
						{
							// Parça bilgilerinden yeni bir WTPart nesnesi oluştur
							WTPart wtPartEntity = new WTPart
							{
								ParcaPartID = !string.IsNullOrEmpty(request.ParcaPartID) ? long.Parse(request.ParcaPartID) : 0,
								ParcaPartMasterID = !string.IsNullOrEmpty(request.ParcaPartMasterID) ? long.Parse(request.ParcaPartMasterID) : 0,
								ParcaName = request.ParcaName ?? string.Empty,
								ParcaNumber = request.ParcaNumber ?? string.Empty,
								ParcaVersion = request.ParcaVersion ?? string.Empty,
								ParcaState = request.ParcaState ?? "RELEASED",
								EntegrasyonDurum = 2, // Hata durumu
								LogMesaj = errorMessage,
								LogDate = DateTime.Now
							};

							// Hata tablosuna aktar
							await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, errorMessage);
							_logger.LogInformation("Parça hata tablosuna aktarıldı. ParcaPartID: {ParcaPartID}", request.ParcaPartID);
						}
						catch (Exception innerEx)
						{
							_logger.LogError(innerEx, "Parça hata tablosuna aktarılırken hata oluştu. ParcaPartID: {ParcaPartID}",
								request.ParcaPartID);
						}
					}

					return new ProcessWTPartReleasedResponse
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
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;
//using Domain.Entities.IntegrationSettings;
//using Application.Interfaces.ApiService;
//using System.Dynamic;
//using Application.Pipelines.Transaction;
//using Domain.Entities; // WTPart, RoleMapping, RoleMappingEndpoint gibi entity'ler burada tanımlı

//namespace Application.Features.WindchillIntegration.WTPartReleased.Commands.Process
//{
//	public class ProcessWTPartReleasedCommand : IRequest<ProcessWTPartReleasedResponse>, IWTPartLoggableRequest, ITransactionalRequest
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

//		public ProcessWTPartReleasedCommand()
//		{
//			LogMessage = "WTPart Released işlemi başlatıldı.";
//			ParcaState = "RELEASED";
//			ParcaPartID = string.Empty;
//			ParcaPartMasterID = string.Empty;
//			ParcaName = string.Empty;
//			ParcaNumber = string.Empty;
//			ParcaVersion = string.Empty;
//			ActionType = "ProcessWTPartReleased";
//			EntegrasyonDurum = "Parca islemde";
//		}

//		public class ProcessWTPartReleasedCommandHandler : IRequestHandler<ProcessWTPartReleasedCommand, ProcessWTPartReleasedResponse>
//		{
//			private readonly IWTPartService<WTPart> _wTPartService;
//			private readonly IApiClientService _apiClientService;
//			private readonly IStateService _stateService;
//			private readonly IIntegrationSettingsService _integrationSettingsService;
//			private readonly IMapper _mapper;
//			private readonly IHttpClientFactory _httpClientFactory;

//			public ProcessWTPartReleasedCommandHandler(
//				IWTPartService<WTPart> wTPartService,
//				IStateService stateService,
//				IMapper mapper,
//				IIntegrationSettingsService integrationSettingsService,
//				IHttpClientFactory httpClientFactory,
//				IApiClientService apiClientService)
//			{
//				_wTPartService = wTPartService;
//				_stateService = stateService;
//				_mapper = mapper;
//				_integrationSettingsService = integrationSettingsService;
//				_httpClientFactory = httpClientFactory;
//				_apiClientService = apiClientService;
//			}

//			public async Task<ProcessWTPartReleasedResponse> Handle(ProcessWTPartReleasedCommand request, CancellationToken cancellationToken)
//			{
//				// 1. Modül ayarlarını kontrol ediyoruz.
//				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
//				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
//				{
//					return new ProcessWTPartReleasedResponse
//					{
//						Success = false,
//						Message = "WTPartReleased modülü pasif durumda."
//					};
//				}

//				// 2. İşlenecek parçayı çekiyoruz.
//				var wtPartEntity = await _stateService.RELEASED(cancellationToken);
//				if (wtPartEntity == null)
//				{
//					return new ProcessWTPartReleasedResponse
//					{
//						Success = false,
//						Message = "Released durumunda veri bulunamadı."
//					};
//				}

//				// 3. Loglama alanlarını güncelliyoruz.
//				request.ParcaPartID = wtPartEntity.ParcaPartID.ToString();
//				request.ParcaPartMasterID = wtPartEntity.ParcaPartMasterID.ToString();
//				request.ParcaName = wtPartEntity.ParcaName;
//				request.ParcaNumber = wtPartEntity.ParcaNumber;
//				request.ParcaVersion = wtPartEntity.ParcaVersion;

//				// 3.1. Gerekli alanları dolduruyoruz: örneğin LogDate ve EntegrasyonDurum
//				wtPartEntity.LogDate = DateTime.Now;
//				wtPartEntity.EntegrasyonDurum = 1; // 1 = başarılı

//				// 4. Rol mapping bilgisini, ProcessTagID = 1 (WTPartReleased) olarak alıyoruz.
//				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(1);
//				if (roleMapping == null || !roleMapping.IsActive)
//				{
//					return new ProcessWTPartReleasedResponse
//					{
//						Success = false,
//						Message = "WTPartReleased rol ayarı bulunamadı veya pasif durumda."
//					};
//				}

//				#region Dinamik AAttribute gönderme çalışmaası


//				string windchillUrl = $"/Windchill/servlet/odata/ProdMgmt/Parts('OR:wt.part.WTPart:{wtPartEntity.ParcaPartID}')";
//				string windchillJson = await _apiClientService.GetAsync<string>(windchillUrl);
//				using var jsonDoc = JsonDocument.Parse(windchillJson);
//				var rootElement = jsonDoc.RootElement;

//				// 6. Dinamik DTO'yu oluşturuyoruz. (Sadece rol ayarlarında tanımlı olan WindchillAttributes alanları alınacak)
//				IDictionary<string, object> dynamicDto = new ExpandoObject();

//				if (roleMapping.WindchillAttributes != null && roleMapping.WindchillAttributes.Any())
//				{
//					foreach (var attribute in roleMapping.WindchillAttributes)
//					{
//						// Windchill API cevabında attribute adının yer alıp almadığını kontrol ediyoruz.
//						if (rootElement.TryGetProperty(attribute.AttributeName, out JsonElement jsonValue))
//						{
//							// JSON değeri string ise alınır.
//							if (jsonValue.ValueKind == JsonValueKind.String)
//							{
//								dynamicDto[attribute.AttributeName] = jsonValue.GetString();
//							}
//							// Eğer jsonValue nesne veya dizi ise, gerçek nesneye deserialize ederek ekliyoruz.
//							else if (jsonValue.ValueKind == JsonValueKind.Object || jsonValue.ValueKind == JsonValueKind.Array)
//							{
//								dynamicDto[attribute.AttributeName] = JsonSerializer.Deserialize<object>(jsonValue.GetRawText());
//							}
//							else
//							{
//								// Diğer veri tipleri için ToString() kullanılır.
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


//				// 5. Rol mapping'in endpoints'lerine veriyi gönderiyoruz.
//				bool allEndpointsSucceeded = true;
//				if (roleMapping.Endpoints != null)
//				{
//					foreach (var endpoint in roleMapping.Endpoints)
//					{
//						// Müşterinin API adresini, roleMapping içerisindeki TargetApi ve Endpoint değerlerinin birleşimi olarak oluşturuyoruz.
//						var targetUrl = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
//						try
//						{
//							var client = _httpClientFactory.CreateClient();
//							// Artık wtPartEntity yerine, dinamik DTO (dynamicDto) gönderilecek.
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


//				// 6. Eğer tüm endpointlere gönderim başarılı ise, parça silinsin.
//				if (allEndpointsSucceeded)
//				{
//					await _wTPartService.DeleteReleasedPartAsync(wtPartEntity, permanent: false);
//					request.LogMessage = "Released işlem başarılı şekilde tamamlandı ve parça silindi.";
//				}
//				else
//				{
//					string errorMessage = "Released işleminde hata oluştu, bir veya daha fazla endpoint'e gönderim başarısız.";
//					await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, errorMessage);

//					request.LogMessage = "Released işleminde hata oluştu, parça hata tablosuna aktarıldı.";
//				}

//				// 7. Yanıt DTO'sunu oluşturuyoruz.
//				var responseDto = _mapper.Map<ProcessWTPartReleasedResponse>(wtPartEntity);
//				responseDto.Success = allEndpointsSucceeded;
//				responseDto.Message = allEndpointsSucceeded
//					? "Released işlem başarılı şekilde tamamlandı."
//					: "Released işleminde hata oluştu, parça gönderilemedi.";
//				return responseDto;
//			}
//		}
//	}
//}

#endregion




#region Denenecek kod hatalari 1 kez yazmak için 

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
//using Domain.Entities.IntegrationSettings; // WTPart, RoleMapping, RoleMappingEndpoint gibi entity'ler burada tanımlı

//namespace Application.Features.WindchillIntegration.WTPartReleased.Commands.Process
//{
//	public class ProcessWTPartReleasedCommand : IRequest<ProcessWTPartReleasedResponse>, IWTPartLoggableRequest
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

//		public ProcessWTPartReleasedCommand()
//		{
//			LogMessage = "WTPart Released işlemi başlatıldı.";
//			ParcaState = "RELEASED";
//			ParcaPartID = string.Empty;
//			ParcaPartMasterID = string.Empty;
//			ParcaName = string.Empty;
//			ParcaNumber = string.Empty;
//			ParcaVersion = string.Empty;
//			ActionType = "ProcessWTPartReleased";
//			EntegrasyonDurum = "Parca islemde";
//		}

//		public class ProcessWTPartReleasedCommandHandler : IRequestHandler<ProcessWTPartReleasedCommand, ProcessWTPartReleasedResponse>
//		{
//			private readonly IWTPartService _wTPartService;
//			private readonly IStateService _stateService;
//			private readonly IIntegrationSettingsService _integrationSettingsService;
//			private readonly IMapper _mapper;
//			private readonly IHttpClientFactory _httpClientFactory;

//			public ProcessWTPartReleasedCommandHandler(
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

//			public async Task<ProcessWTPartReleasedResponse> Handle(ProcessWTPartReleasedCommand request, CancellationToken cancellationToken)
//			{
//				// 1. Modül ayarlarını kontrol ediyoruz.
//				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
//				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
//				{
//					return new ProcessWTPartReleasedResponse
//					{
//						Success = false,
//						Message = "WTPartReleased modülü pasif durumda."
//					};
//				}

//				// 2. İşlenecek parçayı çekiyoruz.
//				var wtPartEntity = await _stateService.RELEASED(cancellationToken);
//				if (wtPartEntity == null)
//				{
//					return new ProcessWTPartReleasedResponse
//					{
//						Success = false,
//						Message = "Released durumunda veri bulunamadı."
//					};
//				}

//				// 2.1. Hata durumundaki kayıtlar için:
//				// Eğer aynı ParcaPartID ve ParcaPartMasterID kombinasyonuna sahip hata kaydı zaten varsa,
//				// bu parçayı tekrar işlemeye almayız.
//				if (await _wTPartService.ErrorRecordExistsAsync(long.Parse(request.ParcaPartID), long.Parse(request.ParcaPartMasterID)))
//				{
//					return new ProcessWTPartReleasedResponse
//					{
//						Success = false,
//						Message = "Bu parça hata olarak zaten işlenmiş. Tekrar işleme alınmayacak."
//					};
//				}

//				// 3. Loglama alanlarını güncelliyoruz.
//				request.ParcaPartID = wtPartEntity.ParcaPartID.ToString();
//				request.ParcaPartMasterID = wtPartEntity.ParcaPartMasterID.ToString();
//				request.ParcaName = wtPartEntity.ParcaName;
//				request.ParcaNumber = wtPartEntity.ParcaNumber;
//				request.ParcaVersion = wtPartEntity.ParcaVersion;

//				// 3.1 Gerekli alanları dolduruyoruz: örneğin LogDate
//				wtPartEntity.LogDate = DateTime.Now;
//				// Başarılı işlemde normalde tekrar loglanmasına engel olmuyor. 
//				// Hata durumunda yukarıdaki kontrol ile tekrar işlem yapılması engellenecek.

//				// 4. Rol mapping bilgisini, ProcessTagID = 1 (WTPartReleased) olarak alıyoruz.
//				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(1);
//				if (roleMapping == null || !roleMapping.IsActive)
//				{
//					return new ProcessWTPartReleasedResponse
//					{
//						Success = false,
//						Message = "WTPartReleased rol ayarı bulunamadı veya pasif durumda."
//					};
//				}

//				// 5. Rol mapping'in endpoints'lerine veriyi gönderiyoruz.
//				bool allEndpointsSucceeded = true;
//				if (roleMapping.Endpoints != null)
//				{
//					foreach (var endpoint in roleMapping.Endpoints)
//					{
//						// Hedef API URL'si: TargetApi ve Endpoint alanlarının birleşimi
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
//							// Hata loglanabilir: ex.Message
//						}
//					}
//				}

//				// 6. Eğer tüm endpointlere gönderim başarılı ise, parça silinsin.
//				if (allEndpointsSucceeded)
//				{
//					await _wTPartService.DeleteReleasedPartAsync(wtPartEntity, permanent: false);
//					request.LogMessage = "Released işlem başarılı şekilde tamamlandı ve parça silindi.";
//				}
//				else
//				{
//					// Hata durumunda ilgili parça daha önce hata olarak kaydedilmemişse, hata kaydı oluşturuyoruz.
//					request.LogMessage = "Released işleminde hata oluştu, parça silinmedi.";

//					// Hata kaydı oluşturmak için servis metodunu çağırıyoruz.
//					await _wTPartService.UpdateReleasedPartAsync(wtPartEntity);
//					// Bu metot içerisinde ParcaPartID ve ParcaPartMasterID kontrolü yapılmalı.
//					// Yani aynı hata kaydı varsa güncelleme yapılmadan atlanmalıdır.
//				}

//				// 7. Yanıt DTO'sunu oluşturuyoruz.
//				var responseDto = _mapper.Map<ProcessWTPartReleasedResponse>(wtPartEntity);
//				responseDto.Success = allEndpointsSucceeded;
//				responseDto.Message = allEndpointsSucceeded
//					? "Released işlem başarılı şekilde tamamlandı."
//					: "Released işleminde hata oluştu, parça gönderilemedi.";
//				return responseDto;
//			}
//		}
//	}
//}
#endregion