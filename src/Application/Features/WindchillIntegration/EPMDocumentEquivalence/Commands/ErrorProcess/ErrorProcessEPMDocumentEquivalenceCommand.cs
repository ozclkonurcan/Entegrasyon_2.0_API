//using Application.Interfaces.Generic;
//using Application.Interfaces.IntegrationSettings;
//using Application.Pipelines.Transaction;
//using Domain.Entities.EPMModels;
//using Domain.Entities.EPMModels.Equivalence;
//using MediatR;
//using System.Dynamic;
//using System.Text;
//using System.Text.Json;

//namespace Application.Features.WindchillIntegration.EPMDocumentEquivalence.Commands.ErrorProcess;

//public class ErrorProcessEPMDocumentEquivalenceCommand : IRequest<ErrorProcessEPMDocumentEquivalenceResponse>, ITransactionalRequest
//{
//	public class Handler : IRequestHandler<ErrorProcessEPMDocumentEquivalenceCommand, ErrorProcessEPMDocumentEquivalenceResponse>
//	{
//		private readonly IRetryService<EPMDocument_Equivalence_Error> _retryService;
//		private readonly IGenericRepository<EPMDocument_Equivalence_Sent> _sentRepo;
//		private readonly IIntegrationSettingsService _settingsService;
//		private readonly IHttpClientFactory _httpClientFactory;

//		public Handler(
//			IRetryService<EPMDocument_Equivalence_Error> retryService,
//			IGenericRepository<EPMDocument_Equivalence_Sent> sentRepo,
//			IIntegrationSettingsService settingsService,
//			IHttpClientFactory httpClientFactory)
//		{
//			_retryService = retryService;
//			_sentRepo = sentRepo;
//			_settingsService = settingsService;
//			_httpClientFactory = httpClientFactory;
//		}

//		public async Task<ErrorProcessEPMDocumentEquivalenceResponse> Handle(ErrorProcessEPMDocumentEquivalenceCommand request, CancellationToken cancellationToken)
//		{
//			try
//			{
//				// 1. Sıradaki Hatalı Kayıt (RetryCount Artırarak)
//				var errorEntity = await _retryService.GetNextAndIncrementAsync(x => true, cancellationToken);
//				if (errorEntity == null) return new ErrorProcessEPMDocumentEquivalenceResponse { Success = false, Message = "Kayıt yok" };

//				// 2. Limit Kontrolü
//				if (_retryService.ShouldDeleteEntity(errorEntity))
//				{
//					await _retryService.DeleteEntityAsync(errorEntity, true, cancellationToken);
//					return new ErrorProcessEPMDocumentEquivalenceResponse { Success = true, Message = "Limit aşıldı, silindi" };
//				}

//				// 3. Rol Mapping (ProcessTagID = 7)
//				var roleMapping = await _settingsService.GetRoleMappingByProcessTagIdAsync(7);
//				if (roleMapping == null || !roleMapping.IsActive)
//				{
//					errorEntity.LogMesaj = "Rol ayarı (ID:7) yok.";
//					await _retryService.UpdateEntityAsync(errorEntity, cancellationToken);
//					return new ErrorProcessEPMDocumentEquivalenceResponse { Success = false, Message = "Rol Hatası" };
//				}

//				// 4. API Gönderimi
//				bool success = true;
//				IDictionary<string, object> dynamicDto = new ExpandoObject();
//				// (DTO Doldurma mantığı Process komutuyla aynı, kısa kesiyorum)
//				if (roleMapping.WindchillAttributes != null)
//				{
//					foreach (var attr in roleMapping.WindchillAttributes)
//					{
//						switch (attr.AttributeName)
//						{
//							case "MainObjectNumber": dynamicDto[attr.AttributeName] = errorEntity.MainObjectNumber; break;
//							case "LinkID": dynamicDto[attr.AttributeName] = errorEntity.LinkID; break;
//							// ... Diğerleri
//							default: dynamicDto[attr.AttributeName] = null; break;
//						}
//					}
//				}

//				if (roleMapping.Endpoints != null)
//				{
//					foreach (var endpoint in roleMapping.Endpoints)
//					{
//						var url = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
//						try
//						{
//							var client = _httpClientFactory.CreateClient("WindchillAPI");
//							var response = await client.PostAsync(url, new StringContent(JsonSerializer.Serialize(dynamicDto), Encoding.UTF8, "application/json"), cancellationToken);
//							if (!response.IsSuccessStatusCode) success = false;
//						}
//						catch { success = false; }
//					}
//				}

//				// 5. Sonuç
//				if (success)
//				{
//					var sent = new EPMDocument_Equivalence_Sent
//					{
//						MainObjectID = errorEntity.MainObjectID,
//						MainObjectNumber = errorEntity.MainObjectNumber,
//						LinkID = errorEntity.LinkID,
//						LogDate = DateTime.Now,
//						EntegrasyonDurum = 1,
//						LogMesaj = "Retry Başarılı"
//						// Diğer alanları da doldur
//					};
//					await _sentRepo.AddAsync(sent);
//					await _retryService.DeleteEntityAsync(errorEntity, true, cancellationToken);
//					return new ErrorProcessEPMDocumentEquivalenceResponse { Success = true, Message = "Başarılı" };
//				}
//				else
//				{
//					errorEntity.LogMesaj = "Tekrar deneme başarısız";
//					await _retryService.UpdateEntityAsync(errorEntity, cancellationToken);
//					return new ErrorProcessEPMDocumentEquivalenceResponse { Success = false, Message = "Hata devam ediyor" };
//				}
//			}
//			catch (Exception ex)
//			{
//				return new ErrorProcessEPMDocumentEquivalenceResponse { Success = false, Message = ex.Message };
//			}
//		}
//	}
//}