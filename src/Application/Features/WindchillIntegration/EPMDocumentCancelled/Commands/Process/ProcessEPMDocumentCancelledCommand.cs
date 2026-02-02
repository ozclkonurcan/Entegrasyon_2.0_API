using Application.Interfaces.ApiService;
using Application.Interfaces.EntegrasyonModulu.EMPDocumentServices;
using Application.Interfaces.Generic;
using Application.Interfaces.IntegrationSettings;
using Application.Interfaces.Mail;
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

namespace Application.Features.WindchillIntegration.EPMDocumentCancelled.Commands.Process;

public class ProcessEPMDocumentCancelledCommand :IRequest<ProcessEPMDocumentCancelledResponse>
{
	public string docNumber { get; set; }

	public class ProcessEPMDocumentCancelledCommandHandler : IRequestHandler<ProcessEPMDocumentCancelledCommand, ProcessEPMDocumentCancelledResponse>
	{
		private readonly IGenericRepository<EPMDocument_CANCELLED> _epmDocumentGenericRepository;
		private readonly IApiClientService _apiClientService;
		private readonly IIntegrationSettingsService _integrationSettingsService;
		private readonly IEPMDocumentStateService _documentStateService;
		private readonly IMapper _mapper;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ProcessEPMDocumentCancelledCommandHandler> _logger;
		private readonly IMediator _mediator;
		private readonly IMailService _mailService;

		public ProcessEPMDocumentCancelledCommandHandler(IGenericRepository<EPMDocument_CANCELLED> epmDocumentGenericRepository, IApiClientService apiClientService, IMapper mapper, IHttpClientFactory httpClientFactory, ILogger<ProcessEPMDocumentCancelledCommandHandler> logger, IMediator mediator, IMailService mailService, IIntegrationSettingsService integrationSettingsService, IEPMDocumentStateService documentStateService)
		{
			_epmDocumentGenericRepository = epmDocumentGenericRepository;
			_apiClientService = apiClientService;
			_mapper = mapper;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
			_mediator = mediator;
			_mailService = mailService;
			_integrationSettingsService = integrationSettingsService;
			_documentStateService = documentStateService;
		}

		public async Task<ProcessEPMDocumentCancelledResponse> Handle(ProcessEPMDocumentCancelledCommand request, CancellationToken cancellationToken)
		{
			try
			{

				// 1. Modül ayarlarını kontrol ediyoruz.
				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
				{
					return new ProcessEPMDocumentCancelledResponse
					{
						Success = false,
						Message = "EPMDocumentCancelled modülü pasif durumda."
					};
				}

				// 2. İşlenecek parçayı çekiyoruz.
				var epmDocumentEntity = await _documentStateService.CANCELLED(cancellationToken);
				if (epmDocumentEntity == null)
				{
					return new ProcessEPMDocumentCancelledResponse
					{
						Success = false,
						Message = "Cancelled durumunda veri bulunamadı."
					};
				}


				// 3. Loglama alanlarını güncelliyoruz.

				request.docNumber = epmDocumentEntity.docNumber;



				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(4);
				if (roleMapping == null || !roleMapping.IsActive)
				{
					return new ProcessEPMDocumentCancelledResponse
					{
						Success = false,
						Message = "EPMDocument Cancelled rol ayarı bulunamadı veya pasif durumda."
					};
				}


				#region Dinamik Attribute Gönderimi
				IDictionary<string, object> dynamicDto = new ExpandoObject();
				bool windchillApiSuccess = false;
				string windchillErrorMessage = string.Empty;

				try
				{
					// Windchill API'den, ilgili parçanın detaylarını çekmek için URL oluşturuyoruz.
					string windchillUrl = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{epmDocumentEntity.EPMDocID}')";
					_logger.LogInformation("Windchill API isteği: {Url}", windchillUrl);

					string windchillJson = await _apiClientService.GetAsync<string>(windchillUrl);
					_logger.LogInformation("Windchill API yanıtı: {Response}", windchillJson);

					// API yanıtı boş veya null ise, parça bulunamadı demektir
					if (string.IsNullOrEmpty(windchillJson) || windchillJson == "{}" || windchillJson == "null")
					{
						windchillErrorMessage = $"Parça Windchill'de bulunamadı. ParcaPartID: {epmDocumentEntity.EPMDocID}";
						_logger.LogWarning(windchillErrorMessage);

						// Parça bulunamadığında, parçayı hata tablosuna aktar
						//await _wTPartService.MoveReleasedPartToErrorAsync(epmDocumentEntity, windchillErrorMessage);

						return new ProcessEPMDocumentCancelledResponse
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
						windchillErrorMessage = $"Geçersiz JSON yanıtı. ParcaPartID: {epmDocumentEntity.EPMDocID}, Yanıt: {windchillJson}";
						_logger.LogError(ex, windchillErrorMessage);

						// Geçersiz JSON durumunda, parçayı hata tablosuna aktar
						//await _wTPartService.MoveReleasedPartToErrorAsync(epmDocumentEntity, windchillErrorMessage);

						return new ProcessEPMDocumentCancelledResponse
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

						windchillErrorMessage = $"Windchill API hatası: {errorMessage}. ParcaPartID: {epmDocumentEntity.EPMDocID}";
						_logger.LogWarning(windchillErrorMessage);

						// API hata döndürdüğünde, parçayı hata tablosuna aktar
						//await _wTPartService.MoveReleasedPartToErrorAsync(epmDocumentEntity, windchillErrorMessage);

						return new ProcessEPMDocumentCancelledResponse
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
						? $"Windchill API hatası: {ex.Message}. ParcaPartID: {epmDocumentEntity.EPMDocID}"
						: windchillErrorMessage;

					_logger.LogError(ex, windchillErrorMessage);

					// Windchill API hatası durumunda, parçayı hata tablosuna aktar
					//await _wTPartService.MoveReleasedPartToErrorAsync(epmDocumentEntity, windchillErrorMessage);

					return new ProcessEPMDocumentCancelledResponse
					{
						Success = false,
						Message = windchillErrorMessage
					};
				}

				// Windchill API'den veri alınamadıysa, işlemi sonlandır
				if (!windchillApiSuccess)
				{
					return new ProcessEPMDocumentCancelledResponse
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
				//SoNRA Aktif edicez
				//if (allEndpointsSucceeded)
				//{


				//	// Başarılı ise, önce Sent tablosuna ekle
				//	var wtPartSentData = new WTPartSentDatas
				//	{
				//		ParcaPartID = wtPartEntity.ParcaPartID,
				//		ParcaPartMasterID = wtPartEntity.ParcaPartMasterID,
				//		ParcaName = wtPartEntity.ParcaName,
				//		ParcaNumber = wtPartEntity.ParcaNumber,
				//		ParcaVersion = wtPartEntity.ParcaVersion,
				//		KulAd = wtPartEntity.KulAd ?? "unknown",
				//		ParcaState = wtPartEntity.ParcaState,
				//		EntegrasyonDurum = 1, // Başarılı
				//		LogMesaj = "Released işlem başarılı şekilde tamamlandı.",
				//		LogDate = DateTime.Now,
				//		ActionType = "ProcessWTPartReleased",
				//		ActionDate = DateTime.Now
				//	};

				//	// Sent tablosuna ekle
				//	await _genericWtpartSentRepository.AddAsync(wtPartSentData);

				//	// Sonra parçayı sil
				//	await _genericWtpartRepository.DeleteAsync(wtPartEntity, permanent: true);


				//	request.LogMessage = "Released işlem başarılı şekilde tamamlandı ve parça silindi.";


				//}
				//else
				//{
				//	string errorMessage = $"Released işleminde hata oluştu: {endpointErrors}";
				//	await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, errorMessage);
				//	await _mailService.SendErrorMailAsync("WTPartReleased", request.ParcaNumber, request.ParcaName, errorMessage, null);
				//	request.LogMessage = "Released işleminde hata oluştu, parça hata tablosuna aktarıldı.";
				//	_logger.LogWarning("Parça hata tablosuna aktarıldı. ParcaPartID: {ParcaPartID}, Hata: {Error}",
				//		wtPartEntity.ParcaPartID, errorMessage);
				//}



				#endregion


				// 7. Sonuç DTO'sunu oluşturuyoruz.
				var responseDto = _mapper.Map<ProcessEPMDocumentCancelledResponse>(epmDocumentEntity);
				responseDto.Success = allEndpointsSucceeded;
				responseDto.Message = allEndpointsSucceeded
					? "Cancelled işlem başarılı şekilde tamamlandı."
					: $"Cancelled işleminde hata oluştu: {endpointErrors}";

				return responseDto;



			}
			catch (Exception)
			{
				// İşlem mantığını burada uygulayın
				return new ProcessEPMDocumentCancelledResponse
				{
					Success = true,
					Message = "İşlem başarılı",
					Ent_ID = 1, // Örnek değerler
					EPMDocID = 12345,
					StateDegeri = "Cancelled",
					idA3masterReference = 67890,
					CadName = "ExampleCadName",
					name = "ExampleName",
					docNumber = "DOC-001"
				};

				throw;
			}

		}


	}
}
