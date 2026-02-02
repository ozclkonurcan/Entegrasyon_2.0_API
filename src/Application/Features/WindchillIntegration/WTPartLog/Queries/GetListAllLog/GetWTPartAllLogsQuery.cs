using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Requests;
using Application.Responses;
using AutoMapper;
using MediatR;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Domain.Entities;
using Application.Interfaces.Generic;

namespace Application.Features.WindchillIntegration.WTPartLog.Queries.GetListAllLog
{
	public class GetWTPartAllLogsQuery : IRequest<GetListResponse<GetWTPartAllLogsDto>>
	{
		public PageRequest PageRequest { get; set; }
		public string? SearchQuery { get; set; } // Arama kelimesi
		public DateTime? StartDate { get; set; } // Başlangıç tarihi
		public DateTime? EndDate { get; set; }   // Bitiş tarihi

		public class GetWTPartAllLogsQueryHandler : IRequestHandler<GetWTPartAllLogsQuery, GetListResponse<GetWTPartAllLogsDto>>
		{
			private readonly IGenericRepository<WTPartAllLogs> _service;
			private readonly IMapper _mapper;

			public GetWTPartAllLogsQueryHandler(IGenericRepository<WTPartAllLogs> service, IMapper mapper)
			{
				_service = service;
				_mapper = mapper;
			}

			public async Task<GetListResponse<GetWTPartAllLogsDto>> Handle(GetWTPartAllLogsQuery request, CancellationToken cancellationToken)
			{
				// Filtreleme için predicate oluşturuluyor
				Expression<Func<WTPartAllLogs, bool>> predicate = x =>
				(string.IsNullOrWhiteSpace(request.SearchQuery) || // SearchQuery boşsa tüm kayıtlar
					(x.ParcaName != null && x.ParcaName.Contains(request.SearchQuery)) ||
					(x.ParcaNumber != null && x.ParcaNumber.Contains(request.SearchQuery)) ||
					(x.KulAd != null && x.KulAd.Contains(request.SearchQuery)) ||
					(x.LogMesaj != null && x.LogMesaj.Contains(request.SearchQuery))) &&
				(!request.StartDate.HasValue || x.LogDate >= request.StartDate.Value.Date) &&
				(!request.EndDate.HasValue || x.LogDate <= request.EndDate.Value.Date.AddDays(1).AddSeconds(-1));

				// Servisten sayfalı veriyi çekiyoruz (burada PageRequest bilgileri kullanılıyor)
				var pagedLogs = await _service.GetListPaginationAsync(
					predicate: predicate,
					orderBy: q => q.OrderByDescending(x => x.LogDate),
					index: request.PageRequest.PageIndex,
					size: request.PageRequest.PageSize,
					cancellationToken: cancellationToken
				);

				// Gelen sayfalı veriyi doğrudan istemciye dönüyoruz
				return new GetListResponse<GetWTPartAllLogsDto>
				{
					Items = _mapper.Map<List<GetWTPartAllLogsDto>>(pagedLogs.Items),
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
