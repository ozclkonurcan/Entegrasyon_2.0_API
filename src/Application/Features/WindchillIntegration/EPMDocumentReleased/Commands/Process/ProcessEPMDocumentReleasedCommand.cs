#region Temize çekilmiş Kod
using Application.Features.WindchillIntegration.WTPartReleased.Commands.Process;
using Application.Interfaces.ApiService;
using Application.Interfaces.EntegrasyonModulu.EMPDocumentServices;
using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Interfaces.Generic;
using Application.Interfaces.IntegrationSettings;
using Application.Interfaces.LogModule;
using Application.Interfaces.Mail;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.EPMModels;
using Domain.Entities.WTPartModels;
using DotNetEnv;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JsonException = Newtonsoft.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Application.Features.WindchillIntegration.EPMDocumentReleased.Commands.Process;

public static class DocumentConstants
{
	public const int RELEASED_STATE = 30;
	public const string DOCUMENT_TYPE_TR = "TR";
	public const string MODIFIED_BY = "WindchillAD";
	public const string NUMBER_PREFIX = "TR_";
}

public class ProcessEPMDocumentReleasedCommand : IRequest<ProcessEPMDocumentReleasedResponse>
{
	public string docNumber { get; set; }

	public class ProcessEPMDocumentReleasedCommandHandler : IRequestHandler<ProcessEPMDocumentReleasedCommand, ProcessEPMDocumentReleasedResponse>
	{
		private readonly IGenericRepository<EPMDocument_RELEASED> _epmDocumentReleasedGenericRepository;
		private readonly IGenericRepository<EPMDocument> _epmDocumentgenericRepository;
		private readonly IGenericRepository<EPMDocumentMaster> _epmDocumentMastergenericRepository;
		private readonly IGenericRepository<EPMReferenceLink> _epmReferenceLinkgenericRepository;
		private readonly IGenericRepository<EPMBuildRule> _epmBuildRulegenericRepository;
		private readonly IGenericRepository<WTPart_Sql> _wtpartSqlgenericRepository;
		private readonly IGenericRepository<WTPartMaster_Sql> _wtpartMasterSqlgenericRepository;
		private readonly IGenericRepository<WTPart> _wtpartgenericRepository;
		private readonly IGenericRepository<WTView> _wtViewgenericRepository;

		// TODO: Bu repository'leri eklemen gerekecek
		// private readonly IGenericRepository<EPMDocument_RELEASED_SENT> _epmDocumentReleasedSentGenericRepository;
		// private readonly IGenericRepository<EPMDocument_RELEASED_ERROR> _epmDocumentReleasedErrorGenericRepository;

		private readonly IApiClientService _apiClientService;
		private readonly IIntegrationSettingsService _integrationSettingsService;
		private readonly IEPMDocumentStateService _documentStateService;
		private readonly IMapper _mapper;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ProcessEPMDocumentReleasedCommandHandler> _logger;
		private readonly IMediator _mediator;
		private readonly IMailService _mailService;

		public ProcessEPMDocumentReleasedCommandHandler(IGenericRepository<EPMDocument_RELEASED> epmDocumentReleasedGenericRepository, IApiClientService apiClientService, IMapper mapper, IHttpClientFactory httpClientFactory, ILogger<ProcessEPMDocumentReleasedCommandHandler> logger, IMediator mediator, IMailService mailService, IIntegrationSettingsService integrationSettingsService, IEPMDocumentStateService documentStateService, IGenericRepository<EPMDocument> epmDocumentgenericRepository, IGenericRepository<EPMReferenceLink> epmReferenceLinkgenericRepository, IGenericRepository<EPMDocumentMaster> epmDocumentMastergenericRepository, IGenericRepository<EPMBuildRule> epmBuildRulegenericRepository, IGenericRepository<WTPart_Sql> wtpartSqlgenericRepository, IGenericRepository<WTPart> wtpartgenericRepository, IGenericRepository<WTPartMaster_Sql> wtpartMasterSqlgenericRepository, IGenericRepository<WTView> wtViewgenericRepository)
		{
			_epmDocumentReleasedGenericRepository = epmDocumentReleasedGenericRepository;
			_apiClientService = apiClientService;
			_mapper = mapper;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
			_mediator = mediator;
			_mailService = mailService;
			_integrationSettingsService = integrationSettingsService;
			_documentStateService = documentStateService;
			_epmDocumentgenericRepository = epmDocumentgenericRepository;
			_epmReferenceLinkgenericRepository = epmReferenceLinkgenericRepository;
			_epmDocumentMastergenericRepository = epmDocumentMastergenericRepository;
			_epmBuildRulegenericRepository = epmBuildRulegenericRepository;
			_wtpartSqlgenericRepository = wtpartSqlgenericRepository;
			_wtpartgenericRepository = wtpartgenericRepository;
			_wtpartMasterSqlgenericRepository = wtpartMasterSqlgenericRepository;
			_wtViewgenericRepository = wtViewgenericRepository;
		}

		public async Task<ProcessEPMDocumentReleasedResponse> Handle(ProcessEPMDocumentReleasedCommand request, CancellationToken cancellationToken)
		{
			try
			{
				// 1. Modül ayarlarını kontrol et
				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
				{
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "EPMDocumentReleased modülü pasif durumda."
					};
				}

				// 2. İşlenecek parçayı çek
				var epmDocumentEntity = await _documentStateService.RELEASED(cancellationToken);
				if (epmDocumentEntity == null)
				{
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "Released durumunda veri bulunamadı."
					};
				}

				request.docNumber = epmDocumentEntity.docNumber;

				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(3);
				if (roleMapping == null || !roleMapping.IsActive)
				{
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "EPMDocument Released rol ayarı bulunamadı veya pasif durumda."
					};
				}

				// 3. Ana işlem
				var result = await ProcessEPMDocumentAsync(epmDocumentEntity, roleMapping, cancellationToken);

				// *** BURASI ÖNEMLİ: İşlem sonucuna göre SENT/ERROR tablosuna aktarım ve ana tablodan silme işlemi ***
				if (result.Success)
				{
					// TODO: SENT tablosuna aktarım işlemi burada olacak
					// await MoveToSentTableAsync(epmDocumentEntity, result.Message);

					// TODO: Ana tablodan silme işlemi burada olacak (SENT'e aktarıldıktan sonra)
					// await _epmDocumentReleasedGenericRepository.DeleteAsync(epmDocumentEntity);
				}
				else
				{
					// TODO: ERROR tablosuna aktarım işlemi burada olacak
					// await MoveToErrorTableAsync(epmDocumentEntity, result.Message);

					// TODO: Ana tablodan silme işlemi burada olacak (ERROR'a aktarıldıktan sonra)
					// await _epmDocumentReleasedGenericRepository.DeleteAsync(epmDocumentEntity);
				}

				if (!result.Success)
				{
					return result;
				}

				// 4. Sonuç DTO'sunu oluştur
				var responseDto = _mapper.Map<ProcessEPMDocumentReleasedResponse>(epmDocumentEntity);
				responseDto.Success = true;
				responseDto.Message = "Released işlem başarılı şekilde tamamlandı.";

				return responseDto;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ProcessEPMDocumentReleased genel hatası");

				// *** EXCEPTION DURUMUNDA ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
				// TODO: Exception durumunda da ERROR tablosuna aktarım yapılacak
				// if (epmDocumentEntity != null)
				// {
				//     await MoveToErrorTableAsync(epmDocumentEntity, $"İşlem hatası: {ex.Message}");
				//     await _epmDocumentReleasedGenericRepository.DeleteAsync(epmDocumentEntity);
				// }

				return new ProcessEPMDocumentReleasedResponse
				{
					Success = false,
					Message = $"İşlem hatası: {ex.Message}"
				};
			}
		}

		private async Task<ProcessEPMDocumentReleasedResponse> ProcessEPMDocumentAsync(EPMDocument_RELEASED epmDocumentEntity, dynamic roleMapping, CancellationToken cancellationToken)
		{
			try
			{
				// Checkout kontrolü
				var checkOutControlQuery = await _epmDocumentgenericRepository.GetAsync(x => x.idA3masterReference == epmDocumentEntity.idA3masterReference && x.statecheckoutInfo == "wrk");
				if (checkOutControlQuery != null)
				{
					// *** CHECKOUT HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "Döküman checkout durumunda."
					};
				}

				if (epmDocumentEntity.StateDegeri != "RELEASED")
				{
					// *** STATE HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = $"Döküman Released durumunda değil. State: {epmDocumentEntity.StateDegeri}"
					};
				}

				// API çağrıları
				string windchillUrlComplete = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{epmDocumentEntity.EPMDocID}')?$expand=Attachments,Representations";
				string cadReferencesJSON = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{epmDocumentEntity.EPMDocID}')/References";

				var completeDataTask = _apiClientService.GetAsync<string>(windchillUrlComplete);
				var referencesTask = _apiClientService.GetAsync<string>(cadReferencesJSON);

				await Task.WhenAll(completeDataTask, referencesTask);

				var windchillCompleteResponse = await completeDataTask;
				var windchillUrlReferencesResponse = await referencesTask;

				var CADResponse = JsonConvert.DeserializeObject<dynamic>(windchillCompleteResponse);
				var cadReferencesJSONResponse = JsonConvert.DeserializeObject<dynamic>(windchillUrlReferencesResponse);

				// Attachments kontrolü
				if (CADResponse?.Attachments == null || CADResponse.Attachments.Count == 0)
				{
					// *** ATTACHMENT BULUNAMADI HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "Attachment bulunamadı."
					};
				}

				if (cadReferencesJSONResponse == null)
				{
					// *** CAD REFERENCES HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "CAD References bulunamadı."
					};
				}

				// Part code al
				string partCode = await GetPartCodeFromReferencesAsync(cadReferencesJSONResponse);
				if (string.IsNullOrEmpty(partCode))
				{
					// *** PART CODE BULUNAMADI HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "Part code bulunamadı."
					};
				}

				// Attachment işle
				var attachmentResult = await ProcessAttachmentsAsync(CADResponse, partCode, roleMapping, cancellationToken);
				return attachmentResult;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ProcessEPMDocumentAsync hatası");
				// *** PROCESS DOCUMENT EXCEPTION - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
				return new ProcessEPMDocumentReleasedResponse
				{
					Success = false,
					Message = $"İşlem hatası: {ex.Message}"
				};
			}
		}

		private async Task<string> GetPartCodeFromReferencesAsync(dynamic cadReferencesJSONResponse)
		{
			try
			{
				var CADReferencesResponse = JObject.Parse(cadReferencesJSONResponse.ToString());
				JArray valueArray = (JArray)CADReferencesResponse["value"] ?? (JArray)CADReferencesResponse["Value"];

				var CADReferencesResponse_ID = valueArray
					.Where(x => x["DepType"]["Display"].ToString() == "Drawing Reference")
					.FirstOrDefault()?["ID"]?.ToString();

				if (string.IsNullOrEmpty(CADReferencesResponse_ID))
					return "";

				string patternReferences = @"OR:wt\.epm\.structure\.EPMReferenceLink:(\d+)";
				var matchReferences = Regex.Match(CADReferencesResponse_ID, patternReferences);

				if (!matchReferences.Success)
					return "";

				var empReferenceLinkID = matchReferences.Groups[1].Value;
				var SQL_EPMReferenceLink = await _epmReferenceLinkgenericRepository.GetAsync(x => x.idA2A2 == long.Parse(empReferenceLinkID));

				if (SQL_EPMReferenceLink == null)
					return "";

				var masterReferenceId = SQL_EPMReferenceLink.idA3B5;

				var documents = await _epmDocumentgenericRepository.GetListAsync(
					predicate: x => x.idA3masterReference == masterReferenceId && x.latestiterationInfo == 1,
					orderBy: q => q
						.OrderByDescending(x => x.versionIdA2versionInfo)
						.ThenByDescending(x => x.versionLevelA2versionInfo)
				);

				var responseEpmDocument = documents.FirstOrDefault();
				if (responseEpmDocument?.statestate != "RELEASED")
					return "";

				var EPMBuildRuleSON = await _apiClientService.GetAsync<string>($"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{responseEpmDocument.idA2A2}')/PartDocAssociations");

				if (string.IsNullOrEmpty(EPMBuildRuleSON))
					return "";

				var buildRuleJsonResponse = JObject.Parse(EPMBuildRuleSON);
				var buildRuleValueArray = buildRuleJsonResponse["value"] as JArray;

				if (buildRuleValueArray == null || buildRuleValueArray.Count == 0)
					return "";

				var firstBuildRuleItem = buildRuleValueArray[0];
				var buildRuleIdValue = firstBuildRuleItem["ID"]?.ToString();

				if (string.IsNullOrEmpty(buildRuleIdValue))
					return "";

				string patternEPMBuildRule = @"OR:wt\.epm\.build\.EPMBuildRule:(\d+)";
				var matchEPMBuildRule = Regex.Match(buildRuleIdValue, patternEPMBuildRule);

				if (!matchEPMBuildRule.Success)
					return "";

				var EPMBuildRuleID = matchEPMBuildRule.Groups[1].Value;
				var SQL_EPMBuildRule = await _epmBuildRulegenericRepository.GetAsync(x => x.idA2A2 == long.Parse(EPMBuildRuleID));

				if (SQL_EPMBuildRule == null)
					return "";

				var properties = SQL_EPMBuildRule.GetType().GetProperties();
				var branchProperty = properties.FirstOrDefault(p =>
					p.Name.ToLower().Contains("branch") ||
					p.Name.Contains("A3B5") ||
					p.Name.ToLower().Contains("branchid"));

				if (branchProperty == null)
					return "";

				var EPMBuildRuleBranchIdA3B5 = Convert.ToInt64(branchProperty.GetValue(SQL_EPMBuildRule));
				var SQL_WTPart = await _wtpartSqlgenericRepository.GetAsync(x =>
					x.branchIditerationInfo == EPMBuildRuleBranchIdA3B5 &&
					x.latestiterationInfo == 1);

				return SQL_WTPart?.idA2A2.ToString() ?? "";
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "GetPartCodeFromReferencesAsync hatası");
				return "";
			}
		}

		private async Task<ProcessEPMDocumentReleasedResponse> ProcessAttachmentsAsync(dynamic CADResponse, string partCode, dynamic roleMapping, CancellationToken cancellationToken)
		{
			try
			{
				// Part bilgilerini al
				var SQL_WTPartControl = await _wtpartSqlgenericRepository.GetAsync(x => x.idA2A2 == long.Parse(partCode));
				if (SQL_WTPartControl == null)
				{
					// *** WTPART BULUNAMADI HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "WTPart bulunamadı."
					};
				}

				var SQL_WTPartMasterControl = await _wtpartMasterSqlgenericRepository.GetAsync(x => x.idA2A2 == SQL_WTPartControl.idA3masterReference);
				var WTViewControl = await _wtViewgenericRepository.GetAsync(x => x.idA2A2 == SQL_WTPartControl.idA3View);

				string partName = SQL_WTPartMasterControl?.name ?? "";
				string partNumber = SQL_WTPartMasterControl?.WTPartNumber ?? "";
				string partState = SQL_WTPartControl?.statestate ?? "";
				string idA3ViewName = WTViewControl?.name ?? "";

				// Windchill WTPart API'den proje kodunu al
				string windchillWTPartApiUrl = $"/Windchill/servlet/odata/ProdMgmt/Parts('OR:wt.part.WTPart:{partCode}')";
				string json = await _apiClientService.GetAsync<string>(windchillWTPartApiUrl);

				if (string.IsNullOrEmpty(json))
				{
					// *** WTPART API VERİ ALINAMADI HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "WTPart API'den veri alınamadı."
					};
				}

				var windchillResponse = JsonConvert.DeserializeObject<dynamic>(json);
				string projeKodu = windchillResponse?.ProjeKodu?.ToString() ?? "";

				if (string.IsNullOrEmpty(projeKodu) || projeKodu == "[]")
				{
					// *** PROJECT CODE BULUNAMADI HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "WTPart'ın projectCode Attr. de değer bulunmuyor."
					};
				}

				string cleanProjectCode = CleanProjectCode(projeKodu);

				// Attachment bul ve işle
				var selectedAttachment = FindPdfAttachment(CADResponse);
				if (selectedAttachment == null)
				{
					// *** PDF ATTACHMENT BULUNAMADI HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "PDF attachment bulunamadı."
					};
				}

				var pdfUrl = selectedAttachment.Content?.URL?.ToString();
				var pdfFileName = selectedAttachment.Content?.Label?.ToString();

				if (string.IsNullOrEmpty(pdfUrl))
				{
					// *** PDF URL BULUNAMADI HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "PDF URL bulunamadı."
					};
				}

				// PDF'i base64'e çevir
				string pdfBase64 = await DownloadAndConvertToBase64Async(pdfUrl);
				if (string.IsNullOrEmpty(pdfBase64))
				{
					// *** PDF İNDİRİLEMEDİ HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
					return new ProcessEPMDocumentReleasedResponse
					{
						Success = false,
						Message = "PDF indirilemedi."
					};
				}

				// CAD View Response oluştur
				var CADViewResponseContentInfo = CreateCADViewResponse(CADResponse, partName, partNumber, cleanProjectCode, pdfBase64, pdfFileName);

				// Endpoint'lere gönder
				return await SendToEndpointsAsync(CADViewResponseContentInfo, roleMapping, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "ProcessAttachmentsAsync hatası");
				// *** PROCESS ATTACHMENTS EXCEPTION - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
				return new ProcessEPMDocumentReleasedResponse
				{
					Success = false,
					Message = $"Attachment işleme hatası: {ex.Message}"
				};
			}
		}

		private dynamic FindPdfAttachment(dynamic CADResponse)
		{
			try
			{
				string numberStr = CADResponse?.Number?.ToString() ?? "";
				string versionStr = CADResponse?.Version?.ToString() ?? "";
				string searchPattern = $"{numberStr} _ {versionStr}";

				dynamic selectedAttachment = null;
				bool foundMatch = false;

				foreach (var attachment in CADResponse.Attachments)
				{
					try
					{
						string label = attachment?.Content?.Label?.ToString() ?? "";
						if (!string.IsNullOrEmpty(label))
						{
							foundMatch = true;
							if (label.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
							{
								selectedAttachment = attachment;
								break;
							}
						}
					}
					catch (Exception ex)
					{
						_logger.LogWarning($"Attachment kontrol hatası: {ex.Message}");
						continue;
					}
				}

				return foundMatch ? selectedAttachment : null;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "FindPdfAttachment hatası");
				return null;
			}
		}

		private string CleanProjectCode(string projeKodu)
		{
			if (string.IsNullOrEmpty(projeKodu))
				return "";

			return projeKodu
				.Replace("[\r\n", "")
				.Replace("\r\n]", "")
				.Replace("[", "")
				.Replace("]", "")
				.Replace("\"", "")
				.Replace("\r", "")
				.Replace("\n", "")
				.Trim();
		}

		private Dictionary<string, object> CreateCADViewResponse(dynamic CADResponse, string partName, string partNumber, string cleanProjectCode, string pdfBase64, string pdfFileName)
		{
			return new Dictionary<string, object>
			{
				["Number"] = DocumentConstants.NUMBER_PREFIX + (CADResponse?.Number?.ToString() ?? ""),
				["Revizyon"] = GetSafeString(CADResponse?.Revision),
				["DocumentType"] = DocumentConstants.DOCUMENT_TYPE_TR,
				["Description"] = partName ?? "Null",
				["ModifiedOn"] = GetSafeDateTime(CADResponse?.LastModified),
				["AuthorizationDate"] = GetSafeDateTime(CADResponse?.LastModified),
				["ModifiedBy"] = DocumentConstants.MODIFIED_BY,
				["state"] = DocumentConstants.RELEASED_STATE,
				["name"] = pdfFileName ?? "",
				["content"] = pdfBase64 ?? "",
				["projectCode"] = cleanProjectCode,
				["relatedParts"] = new List<Dictionary<string, object>>
				{
					new Dictionary<string, object>
					{
						["RelatedPartName"] = partName ?? "Null",
						["RelatedPartNumber"] = partNumber ?? "Null",
						["isUpdateAndDelete"] = false
					}
				}
			};
		}

		private async Task<ProcessEPMDocumentReleasedResponse> SendToEndpointsAsync(Dictionary<string, object> CADViewResponseContentInfo, dynamic roleMapping, CancellationToken cancellationToken)
		{
			bool allEndpointsSucceeded = true;
			StringBuilder endpointErrors = new StringBuilder();

			if (roleMapping.Endpoints != null)
			{
				foreach (var endpoint in roleMapping.Endpoints)
				{
					var targetUrl = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
					try
					{
						_logger.LogInformation($"Endpoint isteği gönderiliyor: {targetUrl}");

						var client = _httpClientFactory.CreateClient("WindchillAPI");
						var jsonContent = JsonSerializer.Serialize(CADViewResponseContentInfo);
						var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
						var httpResponse = await client.PostAsync(targetUrl, content, cancellationToken);

						var responseContent = await httpResponse.Content.ReadAsStringAsync();

						if (!httpResponse.IsSuccessStatusCode)
						{
							string errorMessage = $"Endpoint {targetUrl} hatası: {httpResponse.StatusCode} - {responseContent}";
							_logger.LogWarning(errorMessage);
							endpointErrors.AppendLine(errorMessage);
							allEndpointsSucceeded = false;

							// *** MOCK API'YE GÖNDERİLEMEDİ - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
							// *** SONRA ANA TABLODAN SİLME İŞLEMİ BURADA OLACAK ***
						}
						else
						{
							_logger.LogInformation($"Endpoint başarılı: {targetUrl} - {httpResponse.StatusCode}");

							// *** MOCK API'YE BAŞARILI ŞEKİLDE GÖNDERİLDİ - SENT TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
							// *** SONRA ANA TABLODAN SİLME İŞLEMİ BURADA OLACAK ***
						}
					}
					catch (HttpRequestException ex) when (ex.InnerException is SocketException socketEx && socketEx.ErrorCode == 10061)
					{
						string errorMessage = $"API bağlantı hatası: {targetUrl} - Bağlantı kurulamadı";
						_logger.LogError(errorMessage);
						endpointErrors.AppendLine(errorMessage);
						allEndpointsSucceeded = false;

						// *** BAĞLANTI HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
						// *** SONRA ANA TABLODAN SİLME İŞLEMİ BURADA OLACAK ***
					}
					catch (HttpRequestException ex)
					{
						string errorMessage = $"API hatası: {targetUrl} - {ex.Message.Split('.')[0]}";
						_logger.LogError(errorMessage);
						endpointErrors.AppendLine(errorMessage);
						allEndpointsSucceeded = false;

						// *** HTTP REQUEST HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
						// *** SONRA ANA TABLODAN SİLME İŞLEMİ BURADA OLACAK ***
					}
					catch (Exception ex)
					{
						string errorMessage = $"Genel hata: {targetUrl} - {ex.Message.Split('.')[0]}";
						_logger.LogError(ex, errorMessage);
						endpointErrors.AppendLine(errorMessage);
						allEndpointsSucceeded = false;

						// *** GENEL EXCEPTION - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
						// *** SONRA ANA TABLODAN SİLME İŞLEMİ BURADA OLACAK ***
					}
				}
			}
			else
			{
				string warningMessage = "Hiçbir endpoint tanımlanmamış.";
				_logger.LogWarning(warningMessage);
				endpointErrors.AppendLine(warningMessage);
				allEndpointsSucceeded = false;

				// *** ENDPOINT TANIMLANMAMIŞ HATASI - ERROR TABLOSUNA AKTARIM İŞLEMİ BURADA OLACAK ***
				// *** SONRA ANA TABLODAN SİLME İŞLEMİ BURADA OLACAK ***
			}

			return new ProcessEPMDocumentReleasedResponse
			{
				Success = allEndpointsSucceeded,
				Message = allEndpointsSucceeded
					? "Released işlem başarılı şekilde tamamlandı."
					: $"Released işleminde hata oluştu: {endpointErrors}"
			};
		}

		private string GetSafeString(dynamic value)
		{
			if (value == null) return "";
			string str = value.ToString();
			if (str == "[]" || str == "[[]]" || str == "null") return "";
			return str;
		}

		private DateTime GetSafeDateTime(dynamic value)
		{
			if (value == null) return DateTime.Now;
			string str = value.ToString();
			if (str == "[]" || str == "[[]]" || string.IsNullOrEmpty(str))
				return DateTime.Now;

			if (DateTime.TryParse(str, out DateTime result))
				return result;

			return DateTime.Now;
		}

		private async Task<string> DownloadAndConvertToBase64Async(string pdfUrl)
		{
			if (string.IsNullOrEmpty(pdfUrl)) return "";

			try
			{
				var client = _httpClientFactory.CreateClient("WindchillAPI");

				// .env'den auth bilgilerini al ve ekle
				var windchillUsername = Environment.GetEnvironmentVariable("Windchill_Username");
				var windchillPassword = Environment.GetEnvironmentVariable("Windchill_Password");

				if (!string.IsNullOrEmpty(windchillUsername) && !string.IsNullOrEmpty(windchillPassword))
				{
					var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{windchillUsername}:{windchillPassword}"));
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
				}

				var response = await client.GetAsync(pdfUrl);

				if (response.IsSuccessStatusCode)
				{
					var pdfBytes = await response.Content.ReadAsByteArrayAsync();
					return Convert.ToBase64String(pdfBytes);
				}
				else
				{
					_logger.LogWarning($"PDF indirilemedi: {response.StatusCode}");
					return "";
				}
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError($"HTTP hatası - PDF indirme: {ex.Message}");
				return "";
			}
			catch (TaskCanceledException ex)
			{
				_logger.LogError($"Timeout hatası - PDF indirme: {ex.Message}");
				return "";
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Beklenmeyen hata - PDF indirme: {ex.Message}");
				return "";
			}
		}

		// *** TODO: Bu method'ları implement etmen gerekecek ***
		/*
		private async Task MoveToSentTableAsync(EPMDocument_RELEASED epmDocument, string message)
		{
			var sentEntity = new EPMDocument_RELEASED_SENT
			{
				// EPMDocument_RELEASED'dan tüm alanları kopyala
				// ProcessedDate = DateTime.Now,
				// Message = message
			};
			await _epmDocumentReleasedSentGenericRepository.AddAsync(sentEntity);
		}

		private async Task MoveToErrorTableAsync(EPMDocument_RELEASED epmDocument, string errorMessage)
		{
			var errorEntity = new EPMDocument_RELEASED_ERROR
			{
				// EPMDocument_RELEASED'dan tüm alanları kopyala
				// ErrorDate = DateTime.Now,
				// ErrorMessage = errorMessage
			};
			await _epmDocumentReleasedErrorGenericRepository.AddAsync(errorEntity);
		}
		*/
	}
}
#endregion
#region Eski Kod
//using Application.Features.WindchillIntegration.WTPartReleased.Commands.Process;
//using Application.Interfaces.ApiService;
//using Application.Interfaces.EntegrasyonModulu.EMPDocumentServices;
//using Application.Interfaces.EntegrasyonModulu.WTPartServices;
//using Application.Interfaces.Generic;
//using Application.Interfaces.IntegrationSettings;
//using Application.Interfaces.LogModule;
//using Application.Interfaces.Mail;
//using AutoMapper;
//using Domain.Entities;
//using Domain.Entities.EPMModels;
//using Domain.Entities.WTPartModels;
//using DotNetEnv;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Dynamic;
//using System.Linq;
//using System.Net.Http.Headers;
//using System.Net.Sockets;
//using System.Text;
//using System.Text.Json;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using JsonException = Newtonsoft.Json.JsonException;
//using JsonSerializer = System.Text.Json.JsonSerializer;

//namespace Application.Features.WindchillIntegration.EPMDocumentReleased.Commands.Process;

//public class ProcessEPMDocumentReleasedCommand : IRequest<ProcessEPMDocumentReleasedResponse>
//{
//	public string docNumber { get; set; }

//	public class ProcessEPMDocumentReleasedCommandHandler : IRequestHandler<ProcessEPMDocumentReleasedCommand, ProcessEPMDocumentReleasedResponse>
//	{
//		private readonly IGenericRepository<EPMDocument_RELEASED> _epmDocumentReleasedGenericRepository;
//		private readonly IGenericRepository<EPMDocument> _epmDocumentgenericRepository;
//		private readonly IGenericRepository<EPMDocumentMaster> _epmDocumentMastergenericRepository;
//		private readonly IGenericRepository<EPMReferenceLink> _epmReferenceLinkgenericRepository;
//		private readonly IGenericRepository<EPMBuildRule> _epmBuildRulegenericRepository;
//		private readonly IGenericRepository<WTPart_Sql> _wtpartSqlgenericRepository;
//		private readonly IGenericRepository<WTPartMaster_Sql> _wtpartMasterSqlgenericRepository;
//		private readonly IGenericRepository<WTPart> _wtpartgenericRepository;
//		private readonly IGenericRepository<WTView> _wtViewgenericRepository;
//		private readonly IApiClientService _apiClientService;
//		private readonly IIntegrationSettingsService _integrationSettingsService;
//		private readonly IEPMDocumentStateService _documentStateService;
//		private readonly IMapper _mapper;
//		private readonly IHttpClientFactory _httpClientFactory;
//		private readonly ILogger<ProcessEPMDocumentReleasedCommandHandler> _logger;
//		private readonly IMediator _mediator;
//		private readonly IMailService _mailService;

//		public ProcessEPMDocumentReleasedCommandHandler(IGenericRepository<EPMDocument_RELEASED> epmDocumentReleasedGenericRepository, IApiClientService apiClientService, IMapper mapper, IHttpClientFactory httpClientFactory, ILogger<ProcessEPMDocumentReleasedCommandHandler> logger, IMediator mediator, IMailService mailService, IIntegrationSettingsService integrationSettingsService, IEPMDocumentStateService documentStateService, IGenericRepository<EPMDocument> epmDocumentgenericRepository, IGenericRepository<EPMReferenceLink> epmReferenceLinkgenericRepository, IGenericRepository<EPMDocumentMaster> epmDocumentMastergenericRepository, IGenericRepository<EPMBuildRule> epmBuildRulegenericRepository, IGenericRepository<WTPart_Sql> wtpartSqlgenericRepository, IGenericRepository<WTPart> wtpartgenericRepository, IGenericRepository<WTPartMaster_Sql> wtpartMasterSqlgenericRepository, IGenericRepository<WTView> wtViewgenericRepository)
//		{
//			_epmDocumentReleasedGenericRepository = epmDocumentReleasedGenericRepository;
//			_apiClientService = apiClientService;
//			_mapper = mapper;
//			_httpClientFactory = httpClientFactory;
//			_logger = logger;
//			_mediator = mediator;
//			_mailService = mailService;
//			_integrationSettingsService = integrationSettingsService;
//			_documentStateService = documentStateService;
//			_epmDocumentgenericRepository = epmDocumentgenericRepository;
//			_epmReferenceLinkgenericRepository = epmReferenceLinkgenericRepository;
//			_epmDocumentMastergenericRepository = epmDocumentMastergenericRepository;
//			_epmBuildRulegenericRepository = epmBuildRulegenericRepository;
//			_wtpartSqlgenericRepository = wtpartSqlgenericRepository;
//			_wtpartgenericRepository = wtpartgenericRepository;
//			_wtpartMasterSqlgenericRepository = wtpartMasterSqlgenericRepository;
//			_wtViewgenericRepository = wtViewgenericRepository;
//		}

//		public async Task<ProcessEPMDocumentReleasedResponse> Handle(ProcessEPMDocumentReleasedCommand request, CancellationToken cancellationToken)
//		{
//			try
//			{

//				// 1. Modül ayarlarını kontrol ediyoruz.
//				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
//				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
//				{
//					return new ProcessEPMDocumentReleasedResponse
//					{
//						Success = false,
//						Message = "EPMDocumentReleased modülü pasif durumda."
//					};
//				}

//				// 2. İşlenecek parçayı çekiyoruz.
//				var epmDocumentEntity = await _documentStateService.RELEASED(cancellationToken);
//				if (epmDocumentEntity == null)
//				{
//					return new ProcessEPMDocumentReleasedResponse
//					{
//						Success = false,
//						Message = "Released durumunda veri bulunamadı."
//					};
//				}


//				// 3. Loglama alanlarını güncelliyoruz.

//				request.docNumber = epmDocumentEntity.docNumber;



//				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(3);
//				if (roleMapping == null || !roleMapping.IsActive)
//				{
//					return new ProcessEPMDocumentReleasedResponse
//					{
//						Success = false,
//						Message = "EPMDocument Released rol ayarı bulunamadı veya pasif durumda."
//					};
//				}


//				#region Dinamik Attribute Gönderimi
//				IDictionary<string, object> dynamicDto = new ExpandoObject();
//				bool windchillApiSuccess = false;
//				string windchillErrorMessage = string.Empty;

//				try
//				{
//					// Windchill API'den, ilgili parçanın detaylarını çekmek için URL oluşturuyoruz.
//					//string windchillUrl = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{epmDocumentEntity.EPMDocID}')";
//					string windchillUrl = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{epmDocumentEntity.EPMDocID}')?$expand=Attachments,Representations";
//					//Entegrasyon Windows Form App den alınanlar
//					#region Entegrasyon Windows Form App den alınanlar
//					//string windchillUrlAttachments = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{epmDocumentEntity.EPMDocID}')?$expand=Attachments";
//					//string windchillUrlRepresentations = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{epmDocumentEntity.EPMDocID}')?$expand=Representations";
//					string cadReferencesJSON = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{epmDocumentEntity.EPMDocID}')/References";
//					string jsonWTUSER = $"/Windchill/servlet/odata/PrincipalMgmt/Users?$select=EMail,Name,FullName";


//					Env.Load();

//					string Windchill_Server = Env.GetString("Windchill_Server");
//					var checkOutControlQuery = await _epmDocumentgenericRepository.GetAsync(x => x.idA3masterReference == epmDocumentEntity.idA3masterReference && x.statecheckoutInfo == "wrk");
//					//var checkOutControlQuery = $"SELECT 1 FROM {Windchill_Server}.EPMDocument WHERE [idA3masterReference] = '{epmDocumentEntity.idA3masterReference}' AND [statecheckoutInfo] = 'wrk'";


//					if (checkOutControlQuery == null)
//					{
//						if (epmDocumentEntity.StateDegeri == "RELEASED")
//						{
//							// Tek API çağrısıyla hem CAD verilerini hem Attachments'ı al
//							string windchillUrlComplete = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{epmDocumentEntity.EPMDocID}')?$expand=Attachments,Representations";

//							// Paralel API çağrıları
//							var completeDataTask = _apiClientService.GetAsync<string>(windchillUrlComplete);
//							var referencesTask = _apiClientService.GetAsync<string>(cadReferencesJSON);

//							await Task.WhenAll(completeDataTask, referencesTask);

//							var windchillCompleteResponse = await completeDataTask;
//							var windchillUrlReferencesResponse = await referencesTask;

//							// Ana CAD verisi artık Attachments'ı da içeriyor
//							var CADResponse = JsonConvert.DeserializeObject<dynamic>(windchillCompleteResponse);
//							var cadReferencesJSONResponse = JsonConvert.DeserializeObject<dynamic>(windchillUrlReferencesResponse);

//							string partCode = "";

//							// Attachments kontrolü - artık aynı response'da
//							if (CADResponse.Attachments != null && CADResponse.Attachments.Count > 0)
//							{
//								if (cadReferencesJSONResponse != null)
//								{
//									try
//									{
//										// Dynamic'i string'e çevir
//										var CADReferencesResponse = JObject.Parse(cadReferencesJSONResponse.ToString());
//										JArray valueArray = (JArray)CADReferencesResponse["value"] ?? (JArray)CADReferencesResponse["Value"];
//										var CADReferencesResponse_ID = valueArray
//											.Where(x => x["DepType"]["Display"].ToString() == "Drawing Reference")
//											.FirstOrDefault()?["ID"]?.ToString();

//										if (!string.IsNullOrEmpty(CADReferencesResponse_ID))
//										{
//											string patternReferences = @"OR:wt\.epm\.structure\.EPMReferenceLink:(\d+)";
//											var matchReferences = Regex.Match(CADReferencesResponse_ID, patternReferences);

//											if (matchReferences.Success)
//											{
//												var empReferenceLinkID = matchReferences.Groups[1].Value;
//												var SQL_EPMReferenceLink = await _epmReferenceLinkgenericRepository.GetAsync(x => x.idA2A2 == long.Parse(empReferenceLinkID));

//												if (SQL_EPMReferenceLink != null)
//												{
//													var masterReferenceId = SQL_EPMReferenceLink.idA3B5; // Dynamic kaldırıldı

//													var documents = await _epmDocumentgenericRepository.GetListAsync(
//														predicate: x => x.idA3masterReference == masterReferenceId && x.latestiterationInfo == 1,
//														orderBy: q => q
//															.OrderByDescending(x => x.versionIdA2versionInfo)
//															.ThenByDescending(x => x.versionLevelA2versionInfo)
//													);

//													var responseEpmDocument = documents.FirstOrDefault();

//													if (responseEpmDocument != null)
//													{
//														var documentMaster = await _epmDocumentMastergenericRepository.GetAsync(x => x.idA2A2 == responseEpmDocument.idA3masterReference);

//														if (responseEpmDocument.statestate == "RELEASED")
//														{
//															var EPMBuildRuleSON = await _apiClientService.GetAsync<string>($"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{responseEpmDocument.idA2A2}')/PartDocAssociations");

//															if (!string.IsNullOrEmpty(EPMBuildRuleSON))
//															{
//																try
//																{
//																	// JObject kullan, dynamic değil
//																	var buildRuleJsonResponse = JObject.Parse(EPMBuildRuleSON);
//																	var buildRuleValueArray = buildRuleJsonResponse["value"] as JArray;

//																	if (buildRuleValueArray != null && buildRuleValueArray.Count > 0)
//																	{
//																		// İlk elemanı al
//																		var firstBuildRuleItem = buildRuleValueArray[0];
//																		var buildRuleIdValue = firstBuildRuleItem["ID"]?.ToString();

//																		if (!string.IsNullOrEmpty(buildRuleIdValue))
//																		{
//																			// Regex ile ID'yi çıkar
//																			string patternEPMBuildRule = @"OR:wt\.epm\.build\.EPMBuildRule:(\d+)";
//																			var matchEPMBuildRule = Regex.Match(buildRuleIdValue, patternEPMBuildRule);

//																			if (matchEPMBuildRule.Success)
//																			{
//																				var EPMBuildRuleID = matchEPMBuildRule.Groups[1].Value;
//																				var SQL_EPMBuildRule = await _epmBuildRulegenericRepository.GetAsync(x => x.idA2A2 == long.Parse(EPMBuildRuleID));

//																				if (SQL_EPMBuildRule != null)
//																				{
//																					// Debug için property'leri listele
//																					var properties = SQL_EPMBuildRule.GetType().GetProperties();
//																					_logger.LogInformation($"EPMBuildRule Properties: {string.Join(", ", properties.Select(p => $"{p.Name}={p.GetValue(SQL_EPMBuildRule)}"))}");

//																					// Property adını bul
//																					var branchProperty = properties.FirstOrDefault(p =>
//																						p.Name.ToLower().Contains("branch") ||
//																						p.Name.Contains("A3B5") ||
//																						p.Name.ToLower().Contains("branchid"));

//																					if (branchProperty != null)
//																					{
//																						var EPMBuildRuleBranchIdA3B5 = Convert.ToInt64(branchProperty.GetValue(SQL_EPMBuildRule));
//																						var SQL_WTPart = await _wtpartSqlgenericRepository.GetAsync(x =>
//																							x.branchIditerationInfo == EPMBuildRuleBranchIdA3B5 &&
//																							x.latestiterationInfo == 1);

//																						if (SQL_WTPart != null)
//																						{
//																							partCode = SQL_WTPart.idA2A2.ToString();
//																						}
//																					}
//																					else
//																					{
//																						_logger.LogWarning("EPMBuildRule'da branch property'si bulunamadı");
//																					}
//																				}
//																			}
//																		}
//																	}
//																	else
//																	{
//																		_logger.LogWarning("BuildRule value array null veya boş");
//																	}
//																}
//																catch (Exception ex)
//																{
//																	_logger.LogError($"BuildRule JSON Parse Hatası: {ex.Message}");
//																	_logger.LogError($"Response: {EPMBuildRuleSON}");
//																}
//															}


//															// Attachment işlemi - artık aynı response'dan geliyor
//															if (CADResponse.Attachments != null && CADResponse.Attachments.Count > 0)
//															{
//																// Dynamic değerleri string'e çevir
//																string numberStr = CADResponse.Number?.ToString() ?? "";
//																string versionStr = CADResponse.Version?.ToString() ?? "";
//																string searchPattern = $"{numberStr} _ {versionStr}";

//																// Attachment arama
//																dynamic selectedAttachment = null;
//																bool foundMatch = false;

//																foreach (var attachment in CADResponse.Attachments)
//																{
//																	try
//																	{
//																		string label = attachment.Content?.Label?.ToString() ?? "";
//																		//Test için kapattım
//																		//if (!string.IsNullOrEmpty(label) && label.Contains(searchPattern))
//																		if (!string.IsNullOrEmpty(label))
//																		{
//																			foundMatch = true;
//																			if (label.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
//																			{
//																				selectedAttachment = attachment;
//																				break;
//																			}
//																		}
//																	}
//																	catch (Exception ex)
//																	{
//																		_logger.LogWarning($"Attachment kontrol hatası: {ex.Message}");
//																		continue;
//																	}
//																}

//																if (foundMatch && selectedAttachment != null)
//																{
//																	var pdfUrl = selectedAttachment.Content?.URL?.ToString();
//																	var pdfFileName = selectedAttachment.Content?.Label?.ToString();

//																	if (!string.IsNullOrEmpty(partCode))
//																	{
//																		var SQL_WTPartControl = await _wtpartSqlgenericRepository.GetAsync(x => x.idA2A2 == long.Parse(partCode));

//																		if (SQL_WTPartControl != null)
//																		{

//																			var partName = "";
//																			var partNumber = "";
//																			var partState = "";
//																			var projeCode = "";
//																			var idA3ViewName = "";
//																			var json = "";

//																			var SQL_WTPartMasterControl = await _wtpartMasterSqlgenericRepository.GetAsync(x => x.idA2A2 == SQL_WTPartControl.idA3masterReference);


//																			partName = SQL_WTPartMasterControl.name;
//																			partNumber = SQL_WTPartMasterControl.WTPartNumber;
//																			partState = SQL_WTPartControl.statestate;


//																			var WTViewControl = await _wtViewgenericRepository.GetAsync(x => x.idA2A2 == SQL_WTPartControl.idA3View);
//																			idA3ViewName = WTViewControl.name;


//																			string windchillWTPartApiUrl = $"/Windchill/servlet/odata/ProdMgmt/Parts('OR:wt.part.WTPart:{partCode}')";

//																			json = await _apiClientService.GetAsync<string>(windchillWTPartApiUrl);


//																			if (!string.IsNullOrEmpty(json))
//																			{
//																				var response = JsonConvert.DeserializeObject<dynamic>(json);

//																				// Dynamic'i string'e cast et
//																				string projeKodu = response.ProjeKodu?.ToString();

//																				if (string.IsNullOrEmpty(projeKodu) || projeKodu == "[]")
//																				{
//																					var jsonData4 = JsonConvert.SerializeObject(CADResponse);
//																					return new ProcessEPMDocumentReleasedResponse
//																					{
//																						Success = false,
//																						Message = $"WTPart'ın projectCode Attr. de değer bulunmuyor."
//																					};
//																				}
//																				else
//																				{
//																					string projeKoduStr = response.ProjeKodu?.ToString() ?? "";
//																					string cleanProjectCode = projeKoduStr
//																						.Replace("[\r\n", "")
//																						.Replace("\r\n]", "")
//																						.Replace("[", "")
//																						.Replace("]", "")
//																						.Replace("\"", "")
//																						.Replace("\r", "")
//																						.Replace("\n", "")
//																						.Trim();

//																					string pdfBase64 = await DownloadAndConvertToBase64Async(pdfUrl);
//																					var CADViewResponseContentInfo = new Dictionary<string, object>
//																					{
//																						["Number"] = "TR_" + (CADResponse.Number?.ToString() ?? ""),
//																						["Revizyon"] = (CADResponse.Revision?.ToString() == "[]") ? "" : (CADResponse.Revision?.ToString() ?? ""),
//																						["DocumentType"] = "TR",
//																						["Description"] = partName ?? "Null",
//																						["ModifiedOn"] = (CADResponse.LastModified?.ToString() == "[]") ? DateTime.Now : DateTime.Now, // Geçici
//																						["AuthorizationDate"] = (CADResponse.LastModified?.ToString() == "[]") ? DateTime.Now : DateTime.Now, // Geçici
//																						["ModifiedBy"] = "WindchillAD",
//																						["state"] = 30,
//																						["name"] = pdfFileName ?? "",
//																						["content"] = pdfBase64 ?? "",
//																						["projectCode"] = cleanProjectCode,
//																						["relatedParts"] = new List<Dictionary<string, object>>
//																						{
//																							new Dictionary<string, object>
//																							{
//																								["RelatedPartName"] = partName ?? "Null",
//																								["RelatedPartNumber"] = partNumber ?? "Null",
//																								["isUpdateAndDelete"] = false
//																							}
//																						}
//																					};


//																					if (roleMapping.Endpoints != null && roleMapping.Endpoints.Any())
//																					{
//																						foreach (var endpoint in roleMapping.Endpoints)
//																						{
//																							var targetUrl = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');

//																								_logger.LogInformation("Endpoint isteği gönderiliyor: {Url}", targetUrl);

//																								var client = _httpClientFactory.CreateClient("WindchillAPI");
//																								var jsonContent = JsonSerializer.Serialize(CADViewResponseContentInfo);
//																								var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
//																								var responseTargetApi = await client.PostAsync(targetUrl, content, cancellationToken);

//																								var responseContent = await response.Content.ReadAsStringAsync();


//																								if (!response.IsSuccessStatusCode)
//																								{
//																									string errorMessage = $"Endpoint {targetUrl} hatası: {response.StatusCode} - {responseContent}";
//																									_logger.LogWarning(errorMessage);

//																								}

//																						}
//																					}
//																					else
//																					{
//																						// Endpoint tanımlanmamışsa uyarı ekle
//																						string warningMessage = "Hiçbir endpoint tanımlanmamış.";
//																						_logger.LogWarning(warningMessage);

//																					}
//																				}
//																			}

//																			//Henük bu fonsiyon kurulmadı, ileride aktif edilecek
//																			//await SendPdfToCustomerAttachmentFunctionAsync(pdfUrl, pdfFileName, partItem.EPMDocID, CADResponse, state, partCode);
//																		}
//																	}
//																}
//															}
//														}
//														else
//														{
//															return new ProcessEPMDocumentReleasedResponse
//															{
//																Success = false,
//																Message = $"CAD Döküman Released değil. State: {responseEpmDocument.statestate}"
//															};
//														}
//													}
//												}
//											}
//										}
//									}
//									catch (Exception ex)
//									{
//										_logger.LogError($"JSON Parse Hatası: {ex.Message}");
//										throw;
//									}
//								}
//							}
//						}
//					}

//					#endregion




//					#region Eski apiye istek attığımız kod attachment işlemlerinden vs önce


//					////Entegrasyon Windows Form App den alınanlar
//					//_logger.LogInformation("Windchill API isteği: {Url}", windchillUrl);

//					//string windchillJson = await _apiClientService.GetAsync<string>(windchillUrl);
//					//_logger.LogInformation("Windchill API yanıtı: {Response}", windchillJson);

//					//// API yanıtı boş veya null ise, parça bulunamadı demektir
//					//if (string.IsNullOrEmpty(windchillJson) || windchillJson == "{}" || windchillJson == "null")
//					//{
//					//	windchillErrorMessage = $"Parça Windchill'de bulunamadı. ParcaPartID: {epmDocumentEntity.EPMDocID}";
//					//	_logger.LogWarning(windchillErrorMessage);

//					//	// Parça bulunamadığında, parçayı hata tablosuna aktar
//					//	//await _wTPartService.MoveReleasedPartToErrorAsync(epmDocumentEntity, windchillErrorMessage);

//					//	return new ProcessEPMDocumentReleasedResponse
//					//	{
//					//		Success = false,
//					//		Message = windchillErrorMessage
//					//	};
//					//}

//					//// JSON parse etmeye çalış
//					//JsonDocument jsonDoc;
//					//try
//					//{
//					//	jsonDoc = JsonDocument.Parse(windchillJson);
//					//}
//					//catch (JsonException ex)
//					//{
//					//	windchillErrorMessage = $"Geçersiz JSON yanıtı. ParcaPartID: {epmDocumentEntity.EPMDocID}, Yanıt: {windchillJson}";
//					//	_logger.LogError(ex, windchillErrorMessage);

//					//	// Geçersiz JSON durumunda, parçayı hata tablosuna aktar
//					//	//await _wTPartService.MoveReleasedPartToErrorAsync(epmDocumentEntity, windchillErrorMessage);

//					//	return new ProcessEPMDocumentReleasedResponse
//					//	{
//					//		Success = false,
//					//		Message = windchillErrorMessage
//					//	};
//					//}

//					//var rootElement = jsonDoc.RootElement;

//					//// JSON'da "error" alanı var mı kontrol et (OData hata formatı)
//					//if (rootElement.TryGetProperty("error", out JsonElement errorElement))
//					//{
//					//	string errorMessage = "Bilinmeyen hata";
//					//	if (errorElement.TryGetProperty("message", out JsonElement messageElement))
//					//	{
//					//		errorMessage = messageElement.GetString() ?? errorMessage;
//					//	}

//					//	windchillErrorMessage = $"Windchill API hatası: {errorMessage}. ParcaPartID: {epmDocumentEntity.EPMDocID}";
//					//	_logger.LogWarning(windchillErrorMessage);

//					//	// API hata döndürdüğünde, parçayı hata tablosuna aktar
//					//	//await _wTPartService.MoveReleasedPartToErrorAsync(epmDocumentEntity, windchillErrorMessage);

//					//	return new ProcessEPMDocumentReleasedResponse
//					//	{
//					//		Success = false,
//					//		Message = windchillErrorMessage
//					//	};
//					//}

//					//// Rol ayarlarında tanımlı olan WindchillAttributes değerleriyle dinamik DTO oluşturuyoruz.
//					//if (roleMapping.WindchillAttributes != null && roleMapping.WindchillAttributes.Any())
//					//{
//					//	foreach (var attribute in roleMapping.WindchillAttributes)
//					//	{
//					//		if (rootElement.TryGetProperty(attribute.AttributeName, out JsonElement jsonValue))
//					//		{
//					//			if (jsonValue.ValueKind == JsonValueKind.String)
//					//			{
//					//				dynamicDto[attribute.AttributeName] = jsonValue.GetString();
//					//			}
//					//			else if (jsonValue.ValueKind == JsonValueKind.Object || jsonValue.ValueKind == JsonValueKind.Array)
//					//			{
//					//				dynamicDto[attribute.AttributeName] = JsonSerializer.Deserialize<object>(jsonValue.GetRawText());
//					//			}
//					//			else
//					//			{
//					//				dynamicDto[attribute.AttributeName] = jsonValue.ToString();
//					//			}
//					//		}
//					//		else
//					//		{
//					//			dynamicDto[attribute.AttributeName] = null;
//					//		}
//					//	}
//					//}

//					//windchillApiSuccess = true;
//					#endregion


//				}
//				catch (Exception ex)
//				{
//					windchillErrorMessage = string.IsNullOrEmpty(windchillErrorMessage)
//						? $"Windchill API hatası: {ex.Message}. ParcaPartID: {epmDocumentEntity.EPMDocID}"
//						: windchillErrorMessage;

//					_logger.LogError(ex, windchillErrorMessage);

//					// Windchill API hatası durumunda, parçayı hata tablosuna aktar
//					//await _wTPartService.MoveReleasedPartToErrorAsync(epmDocumentEntity, windchillErrorMessage);

//					return new ProcessEPMDocumentReleasedResponse
//					{
//						Success = false,
//						Message = windchillErrorMessage
//					};
//				}

//				// Windchill API'den veri alınamadıysa, işlemi sonlandır
//				if (!windchillApiSuccess)
//				{
//					return new ProcessEPMDocumentReleasedResponse
//					{
//						Success = false,
//						Message = windchillErrorMessage
//					};
//				}
//				#endregion


//				// 5. Rol mapping'in endpoints'lerine, dinamik DTO'yu gönderiyoruz.
//				bool allEndpointsSucceeded = true;
//				StringBuilder endpointErrors = new StringBuilder();

//				if (roleMapping.Endpoints != null && roleMapping.Endpoints.Any())
//				{
//					foreach (var endpoint in roleMapping.Endpoints)
//					{
//						var targetUrl = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
//						try
//						{
//							_logger.LogInformation("Endpoint isteği gönderiliyor: {Url}", targetUrl);

//							var client = _httpClientFactory.CreateClient("WindchillAPI");
//							var jsonContent = JsonSerializer.Serialize(dynamicDto);
//							var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
//							var response = await client.PostAsync(targetUrl, content, cancellationToken);

//							var responseContent = await response.Content.ReadAsStringAsync();
//							_logger.LogInformation("Endpoint yanıtı: {StatusCode} - {Content}",
//								response.StatusCode, responseContent);

//							if (!response.IsSuccessStatusCode)
//							{
//								string errorMessage = $"Endpoint {targetUrl} hatası: {response.StatusCode} - {responseContent}";
//								_logger.LogWarning(errorMessage);
//								endpointErrors.AppendLine(errorMessage);
//								allEndpointsSucceeded = false;
//							}
//						}
//						catch (HttpRequestException ex) when (ex.InnerException is SocketException socketEx && socketEx.ErrorCode == 10061)
//						{
//							// Bağlantı reddedildi hatası - kısa mesaj
//							string errorMessage = $"API bağlantı hatası: {targetUrl} - Bağlantı kurulamadı";
//							_logger.LogError(errorMessage);
//							endpointErrors.AppendLine(errorMessage);
//							allEndpointsSucceeded = false;
//						}
//						catch (HttpRequestException ex)
//						{
//							// Diğer HTTP hataları - kısa mesaj
//							string errorMessage = $"API hatası: {targetUrl} - {ex.Message.Split('.')[0]}";
//							_logger.LogError(errorMessage);
//							endpointErrors.AppendLine(errorMessage);
//							allEndpointsSucceeded = false;
//						}
//						catch (Exception ex)
//						{
//							// Genel hatalar - kısa mesaj
//							string errorMessage = $"Genel hata: {targetUrl} - {ex.Message.Split('.')[0]}";
//							_logger.LogError(ex, errorMessage);
//							endpointErrors.AppendLine(errorMessage);
//							allEndpointsSucceeded = false;
//						}
//					}
//				}
//				else
//				{
//					// Endpoint tanımlanmamışsa uyarı ekle
//					string warningMessage = "Hiçbir endpoint tanımlanmamış.";
//					_logger.LogWarning(warningMessage);
//					endpointErrors.AppendLine(warningMessage);
//					allEndpointsSucceeded = false;
//				}



//				// 6. İşlem sonucuna göre parçayı sil veya hata tablosuna aktar
//				#region Sentdata guncel
//				//SoNRA Aktif edicez
//				//if (allEndpointsSucceeded)
//				//{


//				//	// Başarılı ise, önce Sent tablosuna ekle
//				//	var wtPartSentData = new WTPartSentDatas
//				//	{
//				//		ParcaPartID = wtPartEntity.ParcaPartID,
//				//		ParcaPartMasterID = wtPartEntity.ParcaPartMasterID,
//				//		ParcaName = wtPartEntity.ParcaName,
//				//		ParcaNumber = wtPartEntity.ParcaNumber,
//				//		ParcaVersion = wtPartEntity.ParcaVersion,
//				//		KulAd = wtPartEntity.KulAd ?? "unknown",
//				//		ParcaState = wtPartEntity.ParcaState,
//				//		EntegrasyonDurum = 1, // Başarılı
//				//		LogMesaj = "Released işlem başarılı şekilde tamamlandı.",
//				//		LogDate = DateTime.Now,
//				//		ActionType = "ProcessWTPartReleased",
//				//		ActionDate = DateTime.Now
//				//	};

//				//	// Sent tablosuna ekle
//				//	await _genericWtpartSentRepository.AddAsync(wtPartSentData);

//				//	// Sonra parçayı sil
//				//	await _genericWtpartRepository.DeleteAsync(wtPartEntity, permanent: true);


//				//	request.LogMessage = "Released işlem başarılı şekilde tamamlandı ve parça silindi.";


//				//}
//				//else
//				//{
//				//	string errorMessage = $"Released işleminde hata oluştu: {endpointErrors}";
//				//	await _wTPartService.MoveReleasedPartToErrorAsync(wtPartEntity, errorMessage);
//				//	await _mailService.SendErrorMailAsync("WTPartReleased", request.ParcaNumber, request.ParcaName, errorMessage, null);
//				//	request.LogMessage = "Released işleminde hata oluştu, parça hata tablosuna aktarıldı.";
//				//	_logger.LogWarning("Parça hata tablosuna aktarıldı. ParcaPartID: {ParcaPartID}, Hata: {Error}",
//				//		wtPartEntity.ParcaPartID, errorMessage);
//				//}



//				#endregion


//				// 7. Sonuç DTO'sunu oluşturuyoruz.
//				var responseDto = _mapper.Map<ProcessEPMDocumentReleasedResponse>(epmDocumentEntity);
//				responseDto.Success = allEndpointsSucceeded;
//				responseDto.Message = allEndpointsSucceeded
//					? "Released işlem başarılı şekilde tamamlandı."
//					: $"Released işleminde hata oluştu: {endpointErrors}";

//				return responseDto;



//			}
//			catch (Exception)
//			{
//				// İşlem mantığını burada uygulayın
//				return new ProcessEPMDocumentReleasedResponse
//				{
//					Success = true,
//					Message = "İşlem başarılı",
//					Ent_ID = 1, // Örnek değerler
//					EPMDocID = 12345,
//					StateDegeri = "Released",
//					idA3masterReference = 67890,
//					CadName = "ExampleCadName",
//					name = "ExampleName",
//					docNumber = "DOC-001"
//				};

//				throw;
//			}

//		}

//		private async Task<string> DownloadAndConvertToBase64Async(string pdfUrl)
//		{
//			if (string.IsNullOrEmpty(pdfUrl)) return "";

//			try
//			{
//				var client = _httpClientFactory.CreateClient("WindchillAPI");

//				// .env'den auth bilgilerini al ve ekle
//				var windchillUsername = Environment.GetEnvironmentVariable("Windchill_Username");
//				var windchillPassword = Environment.GetEnvironmentVariable("Windchill_Password");

//				if (!string.IsNullOrEmpty(windchillUsername) && !string.IsNullOrEmpty(windchillPassword))
//				{
//					var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{windchillUsername}:{windchillPassword}"));
//					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
//				}

//				var response = await client.GetAsync(pdfUrl);

//				if (response.IsSuccessStatusCode)
//				{
//					var pdfBytes = await response.Content.ReadAsByteArrayAsync();
//					return Convert.ToBase64String(pdfBytes);
//				}
//				else
//				{
//					_logger.LogWarning($"PDF indirilemedi: {response.StatusCode}");
//					return "";
//				}
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError($"PDF indirme hatası: {ex.Message}");
//				return "";
//			}
//		}


//	}
//}

#endregion