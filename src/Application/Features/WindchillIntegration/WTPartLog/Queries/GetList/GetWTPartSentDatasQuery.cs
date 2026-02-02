using Application.Features.IntegrationSettings.RoleMappings.Queries.GetList;
using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Interfaces.IntegrationSettings;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartLog.Queries.GetList;

public class GetWTPartSentDatasQuery : IRequest<List<GetWTPartSentDatasDto>>
{

	public class GetWTPartSentDatasQueryHandler : IRequestHandler<GetWTPartSentDatasQuery, List<GetWTPartSentDatasDto>>
	{
		private readonly IWTPartService<WTPart> _service;
		private readonly IMapper _mapper;

		public GetWTPartSentDatasQueryHandler(IWTPartService<WTPart> service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		public async Task<List<GetWTPartSentDatasDto>> Handle(GetWTPartSentDatasQuery request, CancellationToken cancellationToken)
		{
			var entities = await _service.GetWTPartSentDatasAsync();
			return _mapper.Map<List<GetWTPartSentDatasDto>>(entities);
		}
	}
}
