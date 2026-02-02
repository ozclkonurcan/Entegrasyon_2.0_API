using Application.Features.MailService.Commands.SendMail;
using Application.Interfaces.ApiService;
using Application.Interfaces.EntegrasyonModulu.EMPDocumentServices;
using Application.Interfaces.Generic;
using Application.Interfaces.IntegrationSettings;
using Application.Interfaces.Mail;
using Application.Pipelines.Transaction;
using Application.Pipelines.Logging;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.EPMModels;
using Domain.Entities.IntegrationSettings;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Application.Pipelines.EPMDocumentLogging;

namespace Application.Features.WindchillIntegration.EPMDocumentReleased.Commands.Process
{
	public class ProcessEPMDocumentReleasedCommand : IRequest<ProcessEPMDocumentReleasedResponse>, IEPMDocumentLoggableRequest, ITransactionalRequest
	{
		// Loglama ve İşlem Verileri
		public string LogMessage { get; set; }
		public string EPMDocID { get; set; }
		public string DocNumber { get; set; }
		public string CadName { get; set; }
		public string StateDegeri { get; set; }

		public ProcessEPMDocumentReleasedCommand()
		{
			LogMessage = "EPMDocument Released işlemi başlatıldı.";
			StateDegeri = "RELEASED";
			EPMDocID = string.Empty;
			DocNumber = string.Empty;
			CadName = string.Empty;
		}

		public class ProcessEPMDocumentReleasedCommandHandler : IRequestHandler<ProcessEPMDocumentReleasedCommand, ProcessEPMDocumentReleasedResponse>
		{
			// Servisler
			private readonly IEPMDocumentStateService _epmDocumentService;

			// Repositoryler (Sadece Released, Sent ve Error)
			private readonly IGenericRepository<EPMDocument_RELEASED> _genericEpmRepository;
			private readonly IGenericRepository<EPMDocument_SENT> _genericEpmSentRepository;
			private readonly IGenericRepository<EPMDocument_ERROR> _genericEpmErrorRepository;

			private readonly IApiClientService _apiClientService;
			private readonly IIntegrationSettingsService _integrationSettingsService;
			private readonly IHttpClientFactory _httpClientFactory;
			private readonly ILogger<ProcessEPMDocumentReleasedCommandHandler> _logger;
			private readonly IMailService _mailService;
			private readonly IMapper _mapper;

			public ProcessEPMDocumentReleasedCommandHandler(
				IEPMDocumentStateService epmDocumentService,
				IGenericRepository<EPMDocument_RELEASED> genericEpmRepository,
				IGenericRepository<EPMDocument_SENT> genericEpmSentRepository,
				IGenericRepository<EPMDocument_ERROR> genericEpmErrorRepository,
				IIntegrationSettingsService integrationSettingsService,
				IHttpClientFactory httpClientFactory,
				IApiClientService apiClientService,
				ILogger<ProcessEPMDocumentReleasedCommandHandler> logger,
				IMailService mailService,
				IMapper mapper)
			{
				_epmDocumentService = epmDocumentService;
				_genericEpmRepository = genericEpmRepository;
				_genericEpmSentRepository = genericEpmSentRepository;
				_genericEpmErrorRepository = genericEpmErrorRepository;
				_integrationSettingsService = integrationSettingsService;
				_httpClientFactory = httpClientFactory;
				_apiClientService = apiClientService;
				_logger = logger;
				_mailService = mailService;
				_mapper = mapper;
			}

			public async Task<ProcessEPMDocumentReleasedResponse> Handle(ProcessEPMDocumentReleasedCommand request, CancellationToken cancellationToken)
			{
				EPMDocument_RELEASED epmEntity = null;

				try
				{
					// 1. Modül Kontrolü
					var moduleSettings = await _integrationSettingsService.GetModuleSettingsAsync("IntegrationModule");
					if (moduleSettings == null || moduleSettings.SettingsValue == 0)
					{
						return new ProcessEPMDocumentReleasedResponse { Success = false, Message = "EPMDocument modülü pasif." };
					}

					// 2. Veri Çekme (RELEASED Tablosundan)
					// NOT: StateService içindeki metodun dönüş tipi de EPMDocument_RELEASED olmalı!
					epmEntity = await _epmDocumentService.RELEASED(cancellationToken);

					if (epmEntity == null)
					{
						return new ProcessEPMDocumentReleasedResponse { Success = false, Message = "Released durumunda belge bulunamadı." };
					}

					// 3. Command Doldurma (Loglama için)
					request.EPMDocID = epmEntity.EPMDocID.ToString();
					request.DocNumber = epmEntity.docNumber;
					request.CadName = epmEntity.CadName;

					// 4. Rol Mapping (ProcessTagId: 3)
					var roleMapping = await _integrationSettingsService.GetRoleMappingByProcessTagIdAsync(3);
					if (roleMapping == null || !roleMapping.IsActive)
					{
						return new ProcessEPMDocumentReleasedResponse { Success = false, Message = "EPMDocument rol ayarı bulunamadı." };
					}

					#region Windchill API & Dinamik DTO
					IDictionary<string, object> dynamicDto = new ExpandoObject();
					string errorMessage = "";

					try
					{
						// EPMDocument için CAD endpoint'i
						string windchillUrl = $"/Windchill/servlet/odata/CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{epmEntity.EPMDocID}')";
						_logger.LogInformation("Windchill API: {Url}", windchillUrl);

						string windchillJson = await _apiClientService.GetAsync<string>(windchillUrl);

						if (string.IsNullOrEmpty(windchillJson) || windchillJson == "{}" || windchillJson == "null")
						{
							errorMessage = $"Belge Windchill'de bulunamadı. ID: {epmEntity.EPMDocID}";
							await MoveToErrorTableAsync(epmEntity, errorMessage);
							return new ProcessEPMDocumentReleasedResponse { Success = false, Message = errorMessage };
						}

						// JSON Parse ve Hata Kontrolü
						JsonDocument jsonDoc = JsonDocument.Parse(windchillJson);
						var rootElement = jsonDoc.RootElement;

						if (rootElement.TryGetProperty("error", out JsonElement errorElement))
						{
							errorMessage = $"Windchill API Hatası: {errorElement.GetRawText()}";
							await MoveToErrorTableAsync(epmEntity, errorMessage);
							return new ProcessEPMDocumentReleasedResponse { Success = false, Message = errorMessage };
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
						_logger.LogError(ex, errorMessage);
						await MoveToErrorTableAsync(epmEntity, errorMessage);
						return new ProcessEPMDocumentReleasedResponse { Success = false, Message = errorMessage };
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
						endpointErrors.AppendLine("Endpoint tanımlı değil.");
						allEndpointsSucceeded = false;
					}

					// 6. Sonuç İşleme
					if (allEndpointsSucceeded)
					{
						await MoveToSentTableAsync(epmEntity);
						request.LogMessage = "EPMDocument başarıyla işlendi.";
						return new ProcessEPMDocumentReleasedResponse
						{
							Success = true,
							Message = "Başarılı",
							docNumber = epmEntity.docNumber,
							EPMDocID = epmEntity.EPMDocID
						};
					}
					else
					{
						string finalError = endpointErrors.ToString();
						await MoveToErrorTableAsync(epmEntity, finalError);
						await _mailService.SendErrorMailAsync("EPMDocumentReleased", request.DocNumber, request.CadName, finalError, null);
						return new ProcessEPMDocumentReleasedResponse { Success = false, Message = finalError };
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Genel Hata");
					if (epmEntity != null) await MoveToErrorTableAsync(epmEntity, $"Genel Hata: {ex.Message}");
					return new ProcessEPMDocumentReleasedResponse { Success = false, Message = ex.Message };
				}
			}

			// *** YARDIMCI METOTLAR ***
			private async Task MoveToSentTableAsync(EPMDocument_RELEASED entity)
			{
				var sent = new EPMDocument_SENT
				{
					EPMDocID = entity.EPMDocID,
					StateDegeri = entity.StateDegeri,
					idA3masterReference = entity.idA3masterReference,
					CadName = entity.CadName,
					name = entity.name,
					docNumber = entity.docNumber,
					//ProcessDate = DateTime.Now // Tabloda varsa aç
				};
				await _genericEpmSentRepository.AddAsync(sent);
				await _genericEpmRepository.DeleteAsync(entity, permanent: true);
			}

			private async Task MoveToErrorTableAsync(EPMDocument_RELEASED entity, string msg)
			{
				var error = new EPMDocument_ERROR
				{
					EPMDocID = entity.EPMDocID,
					StateDegeri = entity.StateDegeri,
					idA3masterReference = entity.idA3masterReference,
					CadName = entity.CadName,
					name = entity.name,
					docNumber = entity.docNumber,
					//LogMessage = msg, // Tabloda varsa aç
					//ErrorDate = DateTime.Now // Tabloda varsa aç
				};
				await _genericEpmErrorRepository.AddAsync(error);
				await _genericEpmRepository.DeleteAsync(entity, permanent: true);
			}
		}
	}
}