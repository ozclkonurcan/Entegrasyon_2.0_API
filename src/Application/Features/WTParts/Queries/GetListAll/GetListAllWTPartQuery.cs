using Application.Features.WTParts.Queries.GetList;
using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Pipelines.Logging;
using Application.Requests;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WTParts.Queries.GetListAll;

public class GetListAllWTPartQuery : IRequest<List<GetListAllWTPartListItemDto>>, ILoggableRequest
{
	public PageRequest PageRequest { get; set; }
	public string LogMessage => $"WTPart listeleme işlemi gerçekleştirildi.";

	public class GetListAllWTPartQueryHandler : IRequestHandler<GetListAllWTPartQuery, List<GetListAllWTPartListItemDto>>
	{

		private readonly IWTPartService<WTPart> _wTPartService;
		private readonly IMapper _mapper;

		public GetListAllWTPartQueryHandler(IWTPartService<WTPart> wTPartService, IMapper mapper)
		{
			_wTPartService = wTPartService;
			_mapper = mapper;
		}

		public async Task<List<GetListAllWTPartListItemDto>> Handle(GetListAllWTPartQuery request, CancellationToken cancellationToken)
		{
			//var wtParts = await _wTPartService.GetState();
			var wtParts = await _wTPartService.GetState();


			var wtpartListDtoResponse = _mapper.Map<List<GetListAllWTPartListItemDto>>(wtParts);

			return wtpartListDtoResponse;

		}
	}
}
