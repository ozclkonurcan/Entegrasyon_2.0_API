//using Application.Interfaces.Generic;
//using Application.Interfaces.IntegrationSettings;
//using Application.Pipelines.Transaction;
//using Domain.Entities.EPMModels.Equivalence;
//using MediatR;
//using System.Dynamic;
//using System.Text;
//using System.Text.Json;

//namespace Application.Features.WindchillIntegration.EPMDocumentEquivalenceRemoved.Commands.ErrorProcess;

//public class ErrorProcessEPMDocumentEquivalenceRemovedCommand : IRequest<ErrorProcessEPMDocumentEquivalenceRemovedResponse>, ITransactionalRequest
//{
//	public class Handler : IRequestHandler<ErrorProcessEPMDocumentEquivalenceRemovedCommand, ErrorProcessEPMDocumentEquivalenceRemovedResponse>
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

//		public async Task<ErrorProcessEPMDocumentEquivalenceRemovedResponse> Handle(ErrorProcessEPMDocumentEquivalenceRemovedCommand request, CancellationToken cancellationToken)
//		{
//			try
//			{
//				// 1. Sıradaki Hatalı Kayıt (RetryCount Artırarak Getir)
//				var errorEntity = await _retryService.GetNextAndIncrementAsync(x => true, cancellationToken);
//				if (errorEntity == null)
//					return new ErrorProcessEPMDocumentEquivalenceRemovedResponse { Success = false, Message = "İşlenecek hatalı kayıt bulunamadı." };

//				// 2. Limit Kontrolü (MaxRetryCount kontrolü)
//				if (_retryService.ShouldDeleteEntity(errorEntity))
//				{
//					await _retryService.DeleteEntityAsync(errorEntity, true, cancellationToken);
//					return new ErrorProcessEPMDocumentEquivalenceRemovedResponse { Success = true, Message = "Deneme limiti aşıldı, kayıt silindi." };
//				}

//				// 3. Rol Mapping (DİKKAT: Removed işlemi için ID'nin 8 olduğundan emin olun)
//				// ProcessTagID = 8 (Equivalence Removed varsayımı)
//				var roleMapping = await _settingsService.GetRoleMappingByProcessTagIdAsync(8);

//				if (roleMapping == null || !roleMapping.IsActive)
//				{
//					errorEntity.LogMesaj = "İlgili Rol Mapping (ID:8) bulunamadı veya pasif.";
//					await _retryService.UpdateEntityAsync(errorEntity, cancellationToken);
//					return new ErrorProcessEPMDocumentEquivalenceRemovedResponse { Success = false, Message = "Rol Mapping Hatası" };
//				}

//				// 4. API Gönderimi İçin DTO Hazırlığı
//				IDictionary<string, object> dynamicDto = new ExpandoObject();
//				if (roleMapping.WindchillAttributes != null)
//				{
//					foreach (var attr in roleMapping.WindchillAttributes)
//					{
//						switch (attr.AttributeName)
//						{
//							case "MainObjectNumber": dynamicDto[attr.AttributeName] = errorEntity.MainObjectNumber; break;
//							case "LinkID": dynamicDto[attr.AttributeName] = errorEntity.LinkID; break;
//							// Diğer property eşleştirmelerinizin tam olduğundan emin olun
//							// case "CadName": dynamicDto[attr.AttributeName] = errorEntity.CadName; break;
//							default: dynamicDto[attr.AttributeName] = null; break;
//						}
//					}
//				}

//				bool success = true;
//				string lastErrorMessage = "";

//				if (roleMapping.Endpoints != null && roleMapping.Endpoints.Any())
//				{
//					var client = _httpClientFactory.CreateClient("WindchillAPI"); // Client'ı döngü dışında oluşturmak daha sağlıklıdır.

//					foreach (var endpoint in roleMapping.Endpoints)
//					{
//						var url = endpoint.TargetApi.TrimEnd('/') + "/" + endpoint.Endpoint.TrimStart('/');
//						try
//						{
//							var content = new StringContent(JsonSerializer.Serialize(dynamicDto), Encoding.UTF8, "application/json");
//							var response = await client.PostAsync(url, content, cancellationToken);

//							if (!response.IsSuccessStatusCode)
//							{
//								success = false;
//								lastErrorMessage = $"API Hatası ({response.StatusCode}): {url}";
//								break; // Bir endpoint başarısızsa işlemi durduruyoruz.
//							}
//						}
//						catch (Exception ex)
//						{
//							success = false;
//							lastErrorMessage = $"Bağlantı Hatası: {ex.Message}";
//							break;
//						}
//					}
//				}
//				else
//				{
//					// Endpoint yoksa başarılı sayılamaz
//					success = false;
//					lastErrorMessage = "Tanımlı Endpoint bulunamadı.";
//				}

//				// 5. Sonuç İşleme
//				if (success)
//				{
//					var sent = new EPMDocument_Equivalence_Sent
//					{
//						MainObjectID = errorEntity.MainObjectID,
//						MainObjectNumber = errorEntity.MainObjectNumber,
//						LinkID = errorEntity.LinkID,
//						LogDate = DateTime.Now,
//						EntegrasyonDurum = 1,
//						LogMesaj = "Hata tekrarı (Retry) sonucu başarıyla gönderildi."
//						// Buraya Error tablosundaki diğer verileri de taşımanız gerekebilir.
//					};

//					await _sentRepo.AddAsync(sent);
//					await _retryService.DeleteEntityAsync(errorEntity, true, cancellationToken);

//					return new ErrorProcessEPMDocumentEquivalenceRemovedResponse { Success = true, Message = "Başarılı" };
//				}
//				else
//				{
//					// Başarısız ise sadece Log mesajını güncelle ve kaydet (Sayaç zaten step 1'de arttı)
//					errorEntity.LogMesaj = $"Tekrar deneme başarısız. {lastErrorMessage}";
//					await _retryService.UpdateEntityAsync(errorEntity, cancellationToken);

//					return new ErrorProcessEPMDocumentEquivalenceRemovedResponse { Success = false, Message = "Hata devam ediyor: " + lastErrorMessage };
//				}
//			}
//			catch (Exception ex)
//			{
//				// Beklenmeyen genel hata
//				return new ErrorProcessEPMDocumentEquivalenceRemovedResponse { Success = false, Message = $"Critical Exception: {ex.Message}" };
//			}
//		}
//	}
//}