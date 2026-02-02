using Application.Features.MailService.Commands.SendMail;
using Application.Interfaces.ApiService;
using Application.Interfaces.EntegrasyonModulu.EMPDocumentServices;
using Application.Interfaces.Generic;
using Application.Interfaces.IntegrationSettings;
using Application.Interfaces.Mail;
using Application.Pipelines.EPMDocumentLogging; // Loglama Interface'i
using Application.Pipelines.Transaction;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.EPMModels;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocumentCancelled.Commands.Process;

public class ProcessEPMDocumentCancelledCommand : IRequest<ProcessEPMDocumentCancelledResponse>, IEPMDocumentLoggableRequest, ITransactionalRequest
{
	// Loglama için gerekli alanlar
	public string LogMessage { get; set; }
	public string EPMDocID { get; set; }
	public string DocNumber { get; set; }
	public string CadName { get; set; }
	public string StateDegeri { get; set; }

	public ProcessEPMDocumentCancelledCommand()
	{
		LogMessage = "EPMDocument Cancelled işlemi başlatıldı.";
		StateDegeri = "CANCELLED";
		EPMDocID = string.Empty;
		DocNumber = string.Empty;
		CadName = string.Empty;
	}

	public class ProcessEPMDocumentCancelledCommandHandler : IRequestHandler<ProcessEPMDocumentCancelledCommand, ProcessEPMDocumentCancelledResponse>
	{
		private readonly IGenericRepository<EPMDocument_CANCELLED> _epmRepository;
		private readonly IGenericRepository<EPMDocument_CANCELLED_SENT> _epmSentRepository;
		private readonly IGenericRepository<EPMDocument_CANCELLED_ERROR> _epmErrorRepository;

		private readonly IApiClientService _apiClientService;
		private readonly IIntegrationSettingsService _integrationSettingsService;
		private readonly IEPMDocumentStateService _documentStateService;
		private readonly IMapper _mapper;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILogger<ProcessEPMDocumentCancelledCommandHandler> _logger;
		private readonly IMailService _mailService;

		public ProcessEPMDocumentCancelledCommandHandler(
			IGenericRepository<EPMDocument_CANCELLED> epmRepository,
			IGenericRepository<EPMDocument_CANCELLED_SENT> epmSentRepository,
			IGenericRepository<EPMDocument_CANCELLED_ERROR> epmErrorRepository,
			IApiClientService apiClientService,
			IMapper mapper,
			IHttpClientFactory httpClientFactory,
			ILogger<ProcessEPMDocumentCancelledCommandHandler> logger,
			IMediator mediator,
			IMailService mailService,
			IIntegrationSettingsService integrationSettingsService,
			IEPMDocumentStateService documentStateService)
		{
			_epmRepository = epmRepository;
			_epmSentRepository = epmSentRepository;
			_epmErrorRepository = epmErrorRepository;
			_apiClientService = apiClientService;
			_mapper = mapper;
			_httpClientFactory = httpClientFactory;
			_logger = logger;
			_mailService = mailService;
			_integrationSettingsService = integrationSettingsService;
			_documentStateService = documentStateService;
		}

		public async Task<ProcessEPMDocumentCancelledResponse> Handle(ProcessEPMDocumentCancelledCommand request, CancellationToken cancellationToken)
		{
			EPMDocument_CANCELLED epmEntity = null;
			try
			{
				// 1. Modül Kontrolü
				var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
				{
					return new ProcessEPMDocumentCancelledResponse { Success = false, Message = "EPMDocumentCancelled modülü pasif." };
				}

				// 2. Veri Çekme
				epmEntity = await _documentStateService.CANCELLED(cancellationToken);
				if (epmEntity == null)
				{
					return new ProcessEPMDocumentCancelledResponse { Success = false, Message = "Cancelled durumunda veri bulunamadı." };
				}

				// 3. Request Doldurma (Loglama için)
				request.EPMDocID = epmEntity.EPMDocID.ToString();
				request.DocNumber = epmEntity.docNumber;
				request.CadName = epmEntity.CadName;

				// 4. Rol Mapping (Cancelled için ID: 4 demiştin)
				var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(4);
				if (roleMapping == null || !roleMapping.IsActive)
				{
					return new ProcessEPMDocumentCancelledResponse { Success = false, Message = "Rol ayarı bulunamadı." };
				}

				#region Windchill API & Dinamik DTO
				IDictionary<string, object> dynamicDto = new ExpandoObject();
				string windchillErrorMessage = string.Empty;

				try
				{
					string windchillUrl = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{epmEntity.EPMDocID}')";
					_logger.LogInformation("Windchill API: {Url}", windchillUrl);

					string windchillJson = await _apiClientService.GetAsync<string>(windchillUrl);

					if (string.IsNullOrEmpty(windchillJson) || windchillJson == "{}" || windchillJson == "null")
					{
						windchillErrorMessage = $"Belge bulunamadı. ID: {epmEntity.EPMDocID}";
						await MoveToErrorTableAsync(epmEntity, windchillErrorMessage);
						return new ProcessEPMDocumentCancelledResponse { Success = false, Message = windchillErrorMessage };
					}

					JsonDocument jsonDoc = JsonDocument.Parse(windchillJson);
					var rootElement = jsonDoc.RootElement;

					if (rootElement.TryGetProperty("error", out JsonElement errorElement))
					{
						windchillErrorMessage = $"API Hatası: {errorElement.GetRawText()}";
						await MoveToErrorTableAsync(epmEntity, windchillErrorMessage);
						return new ProcessEPMDocumentCancelledResponse { Success = false, Message = windchillErrorMessage };
					}

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
					windchillErrorMessage = $"API Exception: {ex.Message}";
					await MoveToErrorTableAsync(epmEntity, windchillErrorMessage);
					return new ProcessEPMDocumentCancelledResponse { Success = false, Message = windchillErrorMessage };
				}
				#endregion

				// 5. Endpoint Gönderimi
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
								endpointErrors.AppendLine($"Hata ({targetUrl}): {response.StatusCode}");
								allEndpointsSucceeded = false;
							}
						}
						catch (Exception ex)
						{
							endpointErrors.AppendLine($"Hata ({targetUrl}): {ex.Message}");
							allEndpointsSucceeded = false;
						}
					}
				}
				else
				{
					endpointErrors.AppendLine("Endpoint yok.");
					allEndpointsSucceeded = false;
				}

				// 6. Sonuç İşleme
				if (allEndpointsSucceeded)
				{
					await MoveToSentTableAsync(epmEntity);
					request.LogMessage = "Cancelled işlem başarılı.";
					return new ProcessEPMDocumentCancelledResponse
					{
						Success = true,
						Message = "Başarılı",
						docNumber = epmEntity.docNumber,
						EPMDocID = epmEntity.EPMDocID
					};
				}
				else
				{
					string err = endpointErrors.ToString();
					await MoveToErrorTableAsync(epmEntity, err);
					await _mailService.SendErrorMailAsync("EPMDocumentCancelled", request.DocNumber, request.CadName, err, null);
					return new ProcessEPMDocumentCancelledResponse { Success = false, Message = err };
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Genel Hata");
				if (epmEntity != null) await MoveToErrorTableAsync(epmEntity, ex.Message);
				return new ProcessEPMDocumentCancelledResponse { Success = false, Message = ex.Message };
			}
		}

		private async Task MoveToSentTableAsync(EPMDocument_CANCELLED entity)
		{
			var sent = new EPMDocument_CANCELLED_SENT
			{
				EPMDocID = entity.EPMDocID,
				StateDegeri = entity.StateDegeri,
				idA3masterReference = entity.idA3masterReference,
				CadName = entity.CadName,
				name = entity.name,
				docNumber = entity.docNumber
			};
			await _epmSentRepository.AddAsync(sent);
			await _epmRepository.DeleteAsync(entity, permanent: true);
		}

		private async Task MoveToErrorTableAsync(EPMDocument_CANCELLED entity, string msg)
		{
			var error = new EPMDocument_CANCELLED_ERROR
			{
				EPMDocID = entity.EPMDocID,
				StateDegeri = entity.StateDegeri,
				idA3masterReference = entity.idA3masterReference,
				CadName = entity.CadName,
				name = entity.name,
				docNumber = entity.docNumber,
				LogMesaj = msg,
				LogDate = DateTime.Now,
				EntegrasyonDurum = 2,
				RetryCount = 0,
				ActionType = "ProcessEPMDocumentCancelled"
			};
			await _epmErrorRepository.AddAsync(error);
			await _epmRepository.DeleteAsync(entity, permanent: true);
		}
	}
}