using Application.Features.WindchillIntegration.WTPartLog.Queries.GetList;
using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartLog.Queries.GetFilteredList;

public class GetWTPartSentDatasFilteredQuery : IRequest<List<GetWTPartSentDatasDto>>
{
	public string FilterType { get; set; }
	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public string SearchText { get; set; }

	public class GetWTPartSentDatasFilteredQueryHandler : IRequestHandler<GetWTPartSentDatasFilteredQuery, List<GetWTPartSentDatasDto>>
	{
		private readonly IWTPartService<WTPart> _service;
		private readonly IMapper _mapper;

		public GetWTPartSentDatasFilteredQueryHandler(IWTPartService<WTPart> service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		public async Task<List<GetWTPartSentDatasDto>> Handle(GetWTPartSentDatasFilteredQuery request, CancellationToken cancellationToken)
		{
			var allData = await _service.GetWTPartSentDatasAsync();

			var query = allData.AsQueryable();

			if (string.Equals(request.FilterType, "daily", StringComparison.OrdinalIgnoreCase))
			{
				var today = DateTime.Today;
				query = query.Where(x => x.LogDate.Date == today);
			}
			else if (string.Equals(request.FilterType, "custom", StringComparison.OrdinalIgnoreCase)
					 && request.StartDate.HasValue && request.EndDate.HasValue)
			{
				query = query.Where(x => x.LogDate >= request.StartDate.Value && x.LogDate <= request.EndDate.Value);
			}

			if (!string.IsNullOrEmpty(request.SearchText))
			{
				query = query.Where(x =>
					x.ParcaName.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase) ||
					x.ParcaNumber.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase));
			}

			query = query.OrderByDescending(x => x.LogDate);

			var filteredData = query.ToList();

			return _mapper.Map<List<GetWTPartSentDatasDto>>(filteredData);
		}
	}
}
