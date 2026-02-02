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

namespace Application.Features.WTParts.Queries.GetList
{
	public class GetListWTPartQuery : IRequest<GetListResponse<GetListWTPartListItemDto>>,ILoggableRequest
	{
		public PageRequest PageRequest { get; set; }
		public string LogMessage => $"WTPart listeleme işlemi gerçekleştirildi.";

		public class GetListWTPartQueryHandler : IRequestHandler<GetListWTPartQuery, GetListResponse<GetListWTPartListItemDto>>
		{

			private readonly IWTPartService<WTPart> _wTPartService;
			private readonly IMapper _mapper;

			public GetListWTPartQueryHandler(IWTPartService<WTPart> wTPartService, IMapper mapper)
			{
				_wTPartService = wTPartService;
				_mapper = mapper;
			}

			public async Task<GetListResponse<GetListWTPartListItemDto>> Handle(GetListWTPartQuery request, CancellationToken cancellationToken)
			{
				//var wtParts = await _wTPartService.GetState();
				var wtParts = await _wTPartService.GetListAsync(
				index: request.PageRequest.PageIndex,
				size: request.PageRequest.PageSize,
				cancellationToken: cancellationToken
				);


				var wtpartListDtoResponse =  _mapper.Map<GetListResponse<GetListWTPartListItemDto>>(wtParts);

				return wtpartListDtoResponse;

			}
		}
	}
}


