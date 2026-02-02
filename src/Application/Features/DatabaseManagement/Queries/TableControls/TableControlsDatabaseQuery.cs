using Application.Features.DatabaseManagement.Queries.GetList;
using Application.Interfaces.DatabaseManagementModule;
using Application.Responses;
using AutoMapper;
using Domain.Entities.DatabaseManagement;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DatabaseManagement.Queries.TableControls;

public class TableControlsDatabaseQuery : IRequest<List<TableControlsDatabaseListItemDto>>
{
	public class TableControlsDatabaseQueryHandler : IRequestHandler<TableControlsDatabaseQuery, List<TableControlsDatabaseListItemDto>>
	{
		private readonly IDatabaseManagementService _databaseManagementService;
		private readonly IMapper _mapper;

		public TableControlsDatabaseQueryHandler(IDatabaseManagementService databaseManagementService, IMapper mapper)
		{
			_databaseManagementService = databaseManagementService;
			_mapper = mapper;
		}

		public async Task<List<TableControlsDatabaseListItemDto>> Handle(TableControlsDatabaseQuery request, CancellationToken cancellationToken)
		{
			
			// Tabloları getir
			var getTables = await _databaseManagementService.GetTablesAsync();
			var getListDatabaseListItemDto = _mapper.Map<GetListResponse<DatabaseManagementDefinations>>(getTables);

			// Her bir tablo için kontrol işlemi yap
			var tasks = getListDatabaseListItemDto.Items.Select(async item =>
			{
				var tableControlsDTO = await _databaseManagementService.TableControlsAsync(item);
				return _mapper.Map<TableControlsDatabaseListItemDto>(tableControlsDTO); // DatabaseManagementDefinations -> TableControlsDatabaseListItemDto
			}).ToList();

			// Tüm görevleri bekleyip sonuçları al
			var results = await Task.WhenAll(tasks);

			// Sonuçları List<TableControlsDatabaseListItemDto> olarak döndür
			return results.ToList();

			
		}
	}
}

