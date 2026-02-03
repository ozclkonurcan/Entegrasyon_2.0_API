//using Application.Interfaces.EntegrasyonModulu.EMPDocumentServices;
//using Application.Interfaces.Generic;
//using Application.Interfaces.IntegrationSettings;
//using Application.Interfaces.Mail;
//using Application.Pipelines.Transaction;
//using Domain.Entities.EPMModels;
//using Domain.Entities.EPMModels.Equivalence;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Dynamic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Application.Features.WindchillIntegration.EPMDocumentEquivalenceRemoved.Commands.Process;



//public class ProcessEPMDocumentEquivalenceRemovedCommand : IRequest<ProcessEPMDocumentEquivalenceRemovedResponse>, ITransactionalRequest
//{
//	public class Handler : IRequestHandler<ProcessEPMDocumentEquivalenceRemovedCommand, ProcessEPMDocumentEquivalenceRemovedResponse>
//	{
//		private readonly IEPMDocumentEquivalenceService _service;
//		private readonly IGenericRepository<EPMDocument_EquivalenceRemoved> _repo;
//		private readonly IGenericRepository<EPMDocument_EquivalenceRemoved_Sent> _sentRepo;
//		private readonly IGenericRepository<EPMDocument_EquivalenceRemoved_Error> _errorRepo;

//		private readonly IIntegrationSettingsService _settingsService;
//		private readonly IHttpClientFactory _httpClientFactory;
//		private readonly IMailService _mailService;
//		private readonly ILogger<Handler> _logger;

//		public Handler(
//			IEPMDocumentEquivalenceService service,
//			IGenericRepository<EPMDocument_EquivalenceRemoved> repo,
//			IGenericRepository<EPMDocument_EquivalenceRemoved_Sent> sentRepo,
//			IGenericRepository<EPMDocument_EquivalenceRemoved_Error> errorRepo,
//			IIntegrationSettingsService settingsService,
//			IHttpClientFactory httpClientFactory,
//			IMailService mailService,
//			ILogger<Handler> logger)
//		{
//			_service = service;
//			_repo = repo;
//			_sentRepo = sentRepo;
//			_errorRepo = errorRepo;
//			_settingsService = settingsService;
//			_httpClientFactory = httpClientFactory;
//			_mailService = mailService;
//			_logger = logger;
//		}

//		public async Task<ProcessEPMDocumentEquivalenceRemovedResponse> Handle(ProcessEPMDocumentEquivalenceRemovedCommand request, CancellationToken cancellationToken)
//		{
//			EPMDocument_EquivalenceRemoved entity = null;
//			try
//			{
//				// 1. Modül Kontrolü
//				var moduleSettings = await _settingsService.GetModuleSettingsAsync("IntegrationModule");
//				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
//					return new ProcessEPMDocumentEquivalenceRemovedResponse { Success = false, Message = "Modül Pasif" };

//				// 2. Veri Çekme
//				entity = await _service.GetNextEquivalenceRemovedAsync(cancellationToken);
//				if (entity == null)
//					return new ProcessEPMDocumentEquivalenceRemovedResponse { Success = false, Message = "İşlenecek silme kaydı yok." };

//				// 3. Rol Mapping (ProcessTagID = 8)
//				var roleMapping = await _settingsService.GetRoleMappingByProcessTagIdAsync(8);
//				if (roleMapping == null || !roleMapping.IsActive)
//					return new ProcessEPMDocumentEquivalenceRemovedResponse { Success = false, Message = "Rol ayarı (ID:8) bulunamadı." };

//				// 4. DTO Hazırlama
//				IDictionary<string, object> dynamicDto = new ExpandoObject();
//				if (roleMapping.WindchillAttributes != null)
//				{
//					foreach (var attr in roleMapping.WindchillAttributes)
//					{
//						switch (attr.AttributeName)
//						{
//							case "MainObjectNumber": dynamicDto[attr.AttributeName] = entity.MainObjectNumber; break;
//							case "MainObjectVersion": dynamicDto[attr.AttributeName] = entity.MainObjectVersion; break;
//							case "RelatedObjectNumber": dynamicDto[attr.AttributeName] = entity.RelatedObjectNumber; break;
//							case "RelatedObjectVersion": dynamicDto[attr.AttributeName] = entity.RelatedObjectVersion; break;
//							case "LinkID": dynamicDto[attr.AttributeName] = entity.LinkID; break;
//							default: dynamicDto[attr.AttributeName] = null; break;
//						}
//					}
//				}

//				// 5. Endpoint Gönderimi
//				bool success = true;
//				StringBuilder errors = new StringBuilder();

//				if (roleMapping.Endpoints != null && roleMapping.Endpoints.Any())
//				{
//					foreach (var endpoint in roleMapping.Endpoints)
//					{
//						var url = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
//						try
//						{
//							var client = _httpClientFactory.CreateClient("WindchillAPI");
//							var content = new StringContent(JsonSerializer.Serialize(dynamicDto), Encoding.UTF8, "application/json");
//							var response = await client.PostAsync(url, content, cancellationToken);

//							if (!response.IsSuccessStatusCode)
//							{
//								errors.AppendLine($"Hata ({url}): {response.StatusCode}");
//								success = false;
//							}
//						}
//						catch (Exception ex)
//						{
//							errors.AppendLine(ex.Message);
//							success = false;
//						}
//					}
//				}
//				else
//				{
//					errors.AppendLine("Endpoint yok.");
//					success = false;
//				}

//				// 6. Sonuç
//				if (success)
//				{
//					await MoveToSentAsync(entity);
//					return new ProcessEPMDocumentEquivalenceRemovedResponse { Success = true, Message = "Başarılı" };
//				}
//				else
//				{
//					await MoveToErrorAsync(entity, errors.ToString());
//					await _mailService.SendErrorMailAsync("EPMRemoved", entity.MainObjectNumber, "Silme Hatası", errors.ToString(), null);
//					return new ProcessEPMDocumentEquivalenceRemovedResponse { Success = false, Message = errors.ToString() };
//				}
//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Removed Process Error");
//				if (entity != null) await MoveToErrorAsync(entity, ex.Message);
//				return new ProcessEPMDocumentEquivalenceRemovedResponse { Success = false, Message = ex.Message };
//			}
//		}

//		private async Task MoveToSentAsync(EPMDocument_EquivalenceRemoved entity)
//		{
//			var sent = new EPMDocument_EquivalenceRemoved_Sent
//			{
//				MainObjectID = entity.MainObjectID,
//				MainObjectNumber = entity.MainObjectNumber,
//				MainObjectName = entity.MainObjectName,
//				RelatedObjectID = entity.RelatedObjectID,
//				RelatedObjectNumber = entity.RelatedObjectNumber,
//				RelatedObjectName = entity.RelatedObjectName,
//				LinkID = entity.LinkID,
//				LogDate = DateTime.Now,
//				EntegrasyonDurum = 1,
//				LogMesaj = "Başarılı"
//			};
//			await _sentRepo.AddAsync(sent);
//			await _repo.DeleteAsync(entity, permanent: true);
//		}

//		private async Task MoveToErrorAsync(EPMDocument_EquivalenceRemoved entity, string msg)
//		{
//			var error = new EPMDocument_EquivalenceRemoved_Error
//			{
//				MainObjectID = entity.MainObjectID,
//				MainObjectNumber = entity.MainObjectNumber,
//				MainObjectName = entity.MainObjectName,
//				RelatedObjectID = entity.RelatedObjectID,
//				RelatedObjectNumber = entity.RelatedObjectNumber,
//				RelatedObjectName = entity.RelatedObjectName,
//				LinkID = entity.LinkID,
//				LogDate = DateTime.Now,
//				EntegrasyonDurum = 2,
//				LogMesaj = msg,
//				RetryCount = 0,
//				ActionType = "ProcessEPMRemoved"
//			};
//			await _errorRepo.AddAsync(error);
//			await _repo.DeleteAsync(entity, permanent: true);
//		}
//	}
//}