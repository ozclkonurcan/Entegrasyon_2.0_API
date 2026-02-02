using Application.Interfaces.ApiService;
using Application.Interfaces.WindchillModule;
using MediatR;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.WindchillManagement.Queries.GetWindchillAttributes
{
	public class GetWindchillAttributesQuery : IRequest<List<string>>
	{
		public class GetWindchillAttributesQueryHandler : IRequestHandler<GetWindchillAttributesQuery, List<string>>
		{
			private readonly IApiClientService _apiClientService;
			private readonly IWindchillService _tokenService;

			public GetWindchillAttributesQueryHandler(IApiClientService apiClientService, IWindchillService tokenService)
			{
				_apiClientService = apiClientService;
				_tokenService = tokenService;
			}

			public async Task<List<string>> Handle(GetWindchillAttributesQuery request, CancellationToken cancellationToken)
			{
				// Endpoint adresi; base address ApiClientService içinde ayarlanmış olacak.
				string endpoint = "/Windchill/servlet/odata/ProdMgmt/Parts";
				var tokenDto = await _tokenService.GetTokenAsync();
				// Sadece 1 kayıt çekmek için ek header bilgileri
				var headers = new Dictionary<string, string>
				{
					{ "Prefer", "odata.maxpagesize=1" },
					{ "CSRF_NONCE", tokenDto.NonceValue }
				};

				// API’den gelen yanıtı string olarak alıyoruz.
				var jsonResponse = await _apiClientService.GetAsync<string>(endpoint, headers);
				var attributes = new List<string>();

				using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
				{
					var root = doc.RootElement;

					// OData yanıtlarında veriler genellikle "value" propertysi altında gelir.
					if (root.TryGetProperty("value", out JsonElement valueElement) && valueElement.ValueKind == JsonValueKind.Array)
					{
						if (valueElement.GetArrayLength() > 0)
						{
							var firstRecord = valueElement[0];
							foreach (var property in firstRecord.EnumerateObject())
							{
								attributes.Add(property.Name);
							}
						}
					}
					else
					{
						// Eğer kayıt doğrudan root'ta geliyorsa
						foreach (var property in root.EnumerateObject())
						{
							attributes.Add(property.Name);
						}
					}
				}

				return attributes;
			}
		}
	}
}
