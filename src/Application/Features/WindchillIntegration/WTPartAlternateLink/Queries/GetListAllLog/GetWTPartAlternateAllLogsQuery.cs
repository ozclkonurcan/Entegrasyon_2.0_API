using Application.Interfaces.Generic;
using Application.Requests;
using Application.Responses;
using AutoMapper;
using MediatR;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Domain.Entities.WTPartModels.AlternateModels;

namespace Application.Features.WindchillIntegration.WTPartAlternateLink.Queries.GetListAllLog
{
	public class GetWTPartAlternateAllLogsQuery : IRequest<GetListResponse<GetWTPartAlternateAllLogsDto>>
	{
		public PageRequest PageRequest { get; set; }
		public string? SearchQuery { get; set; } // Arama kelimesi
		public DateTime? StartDate { get; set; } // Başlangıç tarihi
		public DateTime? EndDate { get; set; }   // Bitiş tarihi

		public class GetWTPartAlternateAllLogsQueryHandler : IRequestHandler<GetWTPartAlternateAllLogsQuery, GetListResponse<GetWTPartAlternateAllLogsDto>>
		{
			private readonly IGenericRepository<WTPartAlternateLinkLogEntegration> _service;
			private readonly IMapper _mapper;

			public GetWTPartAlternateAllLogsQueryHandler(IGenericRepository<WTPartAlternateLinkLogEntegration> service, IMapper mapper)
			{
				_service = service;
				_mapper = mapper;
			}

			public async Task<GetListResponse<GetWTPartAlternateAllLogsDto>> Handle(GetWTPartAlternateAllLogsQuery request, CancellationToken cancellationToken)
			{
				// Filtreleme için predicate oluşturuluyor
				Expression<Func<WTPartAlternateLinkLogEntegration, bool>> predicate = x =>
				(string.IsNullOrWhiteSpace(request.SearchQuery) || // SearchQuery boşsa tüm kayıtlar
					(x.AnaParcaName != null && x.AnaParcaName.Contains(request.SearchQuery)) ||
					(x.AnaParcaNumber != null && x.AnaParcaNumber.Contains(request.SearchQuery)) ||
					(x.MuadilParcaName != null && x.MuadilParcaName.Contains(request.SearchQuery)) ||
					(x.MuadilParcaNumber != null && x.MuadilParcaNumber.Contains(request.SearchQuery)) ||
					(x.KulAd != null && x.KulAd.Contains(request.SearchQuery)) ||
					(x.LogMesaj != null && x.LogMesaj.Contains(request.SearchQuery))) &&
				(!request.StartDate.HasValue || x.LogDate >= request.StartDate.Value.Date) &&
				(!request.EndDate.HasValue || x.LogDate <= request.EndDate.Value.Date.AddDays(1).AddSeconds(-1));

				// Servisten sayfalı veriyi çekiyoruz
				var pagedLogs = await _service.GetListPaginationAsync(
					predicate: predicate,
					orderBy: q => q.OrderByDescending(x => x.LogDate),
					index: request.PageRequest.PageIndex,
					size: request.PageRequest.PageSize,
					cancellationToken: cancellationToken
				);

				// Gelen sayfalı veriyi doğrudan istemciye dönüyoruz
				return new GetListResponse<GetWTPartAlternateAllLogsDto>
				{
					Items = _mapper.Map<List<GetWTPartAlternateAllLogsDto>>(pagedLogs.Items),
					Index = pagedLogs.Index,
					Size = pagedLogs.Size,
					Count = pagedLogs.Count,
					Pages = pagedLogs.Pages,
					HasNext = pagedLogs.HasNext,
					HasPrevious = pagedLogs.HasPrevious
				};
			}
		}
	}
}