//using Application.Interfaces.ApiService;
//using Application.Interfaces.EntegrasyonModulu.EMPDocumentServices; // Yeni servisi ekle
//using Application.Interfaces.Generic;
//using Application.Interfaces.IntegrationSettings;
//using Application.Interfaces.Mail;
//using Application.Pipelines.EPMDocumentLogging; // Loglama Interface'i (Varsa)
//using Application.Pipelines.Transaction;
//using AutoMapper;
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

//namespace Application.Features.WindchillIntegration.EPMDocumentEquivalence.Commands.Process;


//public class ProcessEPMDocumentEquivalenceCommand : IRequest<ProcessEPMDocumentEquivalenceResponse>, ITransactionalRequest
//{
//	// Handler Başlangıcı
//	public class ProcessEPMDocumentEquivalenceCommandHandler : IRequestHandler<ProcessEPMDocumentEquivalenceCommand, ProcessEPMDocumentEquivalenceResponse>
//	{
//		private readonly IEPMDocumentEquivalenceService _equivService;
//		private readonly IGenericRepository<EPMDocument_Equivalence> _repo;
//		private readonly IGenericRepository<EPMDocument_Equivalence_Sent> _sentRepo;
//		private readonly IGenericRepository<EPMDocument_Equivalence_Error> _errorRepo;

//		private readonly IIntegrationSettingsService _settingsService;
//		private readonly IHttpClientFactory _httpClientFactory;
//		private readonly ILogger<ProcessEPMDocumentEquivalenceCommandHandler> _logger;
//		private readonly IMailService _mailService;

//		public ProcessEPMDocumentEquivalenceCommandHandler(
//			IEPMDocumentEquivalenceService equivService,
//			IGenericRepository<EPMDocument_Equivalence> repo,
//			IGenericRepository<EPMDocument_Equivalence_Sent> sentRepo,
//			IGenericRepository<EPMDocument_Equivalence_Error> errorRepo,
//			IIntegrationSettingsService settingsService,
//			IHttpClientFactory httpClientFactory,
//			ILogger<ProcessEPMDocumentEquivalenceCommandHandler> logger,
//			IMailService mailService)
//		{
//			_equivService = equivService;
//			_repo = repo;
//			_sentRepo = sentRepo;
//			_errorRepo = errorRepo;
//			_settingsService = settingsService;
//			_httpClientFactory = httpClientFactory;
//			_logger = logger;
//			_mailService = mailService;
//		}

//		public async Task<ProcessEPMDocumentEquivalenceResponse> Handle(ProcessEPMDocumentEquivalenceCommand request, CancellationToken cancellationToken)
//		{
//			EPMDocument_Equivalence entity = null;
//			try
//			{
//				// 1. Modül Kontrolü
//				var moduleSettings = await _settingsService.GetModuleSettingsAsync("IntegrationModule");
//				if (moduleSettings == null || moduleSettings.SettingsValue == 0)
//					return new ProcessEPMDocumentEquivalenceResponse { Success = false, Message = "Modül Pasif" };

//				// 2. Veri Çekme (Service üzerinden)
//				entity = await _equivService.GetNextEquivalenceAsync(cancellationToken);
//				if (entity == null)
//					return new ProcessEPMDocumentEquivalenceResponse { Success = false, Message = "İşlenecek ilişki yok." };

//				// 3. Rol Mapping (Örn: ProcessTagID = 5 -> EPM_EQUIVALENCE)
//				// NOT: Veritabanında Des2_RolProcessTags tablosuna 'EPM_EQUIVALENCE' ekleyip ID'sini buraya yazmalısın.
//				var roleMapping = await _settingsService.GetRoleMappingByProcessTagIdAsync(7);
//				if (roleMapping == null || !roleMapping.IsActive)
//				{
//					return new ProcessEPMDocumentEquivalenceResponse { Success = false, Message = "Rol ayarı (ID:5) bulunamadı." };
//				}

//				// 4. DTO Hazırlama (SQL Verisinden)
//				// Not: İlişki tablolarında genellikle Windchill API'ye gitmeye gerek yoktur, veri zaten tablodadır.
//				IDictionary<string, object> dynamicDto = new ExpandoObject();

//				// Müşteri hangi alanları istiyorsa (RoleMapping Attributes) onları dolduruyoruz.
//				if (roleMapping.WindchillAttributes != null)
//				{
//					foreach (var attr in roleMapping.WindchillAttributes)
//					{
//						// AttributeName ile Entity propertylerini eşleştiriyoruz (Reflection veya Manuel)
//						// Manuel Mapping daha performanslıdır:
//						switch (attr.AttributeName)
//						{
//							case "MainObjectNumber": dynamicDto[attr.AttributeName] = entity.MainObjectNumber; break;
//							case "MainObjectVersion": dynamicDto[attr.AttributeName] = entity.MainObjectVersion; break;
//							case "RelatedObjectNumber": dynamicDto[attr.AttributeName] = entity.RelatedObjectNumber; break;
//							case "RelatedObjectVersion": dynamicDto[attr.AttributeName] = entity.RelatedObjectVersion; break;
//							case "LinkID": dynamicDto[attr.AttributeName] = entity.LinkID; break;
//							default: dynamicDto[attr.AttributeName] = null; break; // Bulamazsa null
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
//							errors.AppendLine($"Hata ({url}): {ex.Message}");
//							success = false;
//						}
//					}
//				}
//				else
//				{
//					errors.AppendLine("Endpoint yok.");
//					success = false;
//				}

//				// 6. Sonuç Yönetimi
//				if (success)
//				{
//					await MoveToSentAsync(entity);
//					return new ProcessEPMDocumentEquivalenceResponse { Success = true, Message = "Başarılı", MainNumber = entity.MainObjectNumber, RelatedNumber = entity.RelatedObjectNumber };
//				}
//				else
//				{
//					string err = errors.ToString();
//					await MoveToErrorAsync(entity, err);
//					await _mailService.SendErrorMailAsync("EPMEquivalence", entity.MainObjectNumber, "İlişki Hatası", err, null);
//					return new ProcessEPMDocumentEquivalenceResponse { Success = false, Message = err };
//				}

//			}
//			catch (Exception ex)
//			{
//				_logger.LogError(ex, "Equivalence Process Error");
//				if (entity != null) await MoveToErrorAsync(entity, ex.Message);
//				return new ProcessEPMDocumentEquivalenceResponse { Success = false, Message = ex.Message };
//			}
//		}

//		private async Task MoveToSentAsync(EPMDocument_Equivalence entity)
//		{
//			var sent = new EPMDocument_Equivalence_Sent
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

//		private async Task MoveToErrorAsync(EPMDocument_Equivalence entity, string msg)
//		{
//			var error = new EPMDocument_Equivalence_Error
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
//				ActionType = "ProcessEPMEquivalence"
//			};
//			await _errorRepo.AddAsync(error);
//			await _repo.DeleteAsync(entity, permanent: true);
//		}
//	}
//}