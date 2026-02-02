using Application.Interfaces.DatabaseManagementModule;
using Application.Responses;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DatabaseManagement.Queries.GetList;

public class GetListDatabaseQuery :IRequest<GetListResponse<GetListDatabaseListItemDto>>
{
	public class GetListDatabaseQueryHandler : IRequestHandler<GetListDatabaseQuery, GetListResponse<GetListDatabaseListItemDto>>
	{
		private readonly IDatabaseManagementService _databaseManagementService;
		private readonly IMapper _mapper;

		public GetListDatabaseQueryHandler(IDatabaseManagementService databaseManagementService, IMapper mapper)
		{
			_databaseManagementService = databaseManagementService;
			_mapper = mapper;
		}

		public async Task<GetListResponse<GetListDatabaseListItemDto>> Handle(GetListDatabaseQuery request, CancellationToken cancellationToken)
		{

			var getTables =await _databaseManagementService.GetTablesAsync();
			var getListDatabaseListItemDto = _mapper.Map<GetListResponse<GetListDatabaseListItemDto>>(getTables);

			return getListDatabaseListItemDto;
		}
	}
}
