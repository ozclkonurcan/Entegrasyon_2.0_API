using Application.Features.DatabaseManagement.Queries.TableControls;
using Application.Interfaces.ConnectionModule;
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

namespace Application.Pipelines.RequireSystemCheck;

public class SystemCheckBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>, IRequireSystemCheck
{
	private readonly IDatabaseManagementService _databaseManagementService;
	private readonly IConnectionService _connectionService;
	private readonly IMapper _mapper;
	public SystemCheckBehavior(IDatabaseManagementService databaseManagementService, IConnectionService connectionService, IMapper mapper)
	{
		_databaseManagementService = databaseManagementService;
		_connectionService = connectionService;
		_mapper = mapper;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		if (!await _connectionService.ConnectionControl())
		{
			throw new Exception("Database connection failed.");

		}
		// Tabloları getir
		var getTables = await _databaseManagementService.GetTablesAsync();
		var getListDatabaseListItemDto = _mapper.Map<GetListResponse<DatabaseManagementDefinations>>(getTables);

		// Her bir tablo için kontrol işlemi yap
		var tasks = getListDatabaseListItemDto.Items.Select(async item =>
		{
			var tableControlsDTO = await _databaseManagementService.TableControlsAsync(item);
			return _mapper.Map<TableControlsDatabaseListItemDto>(tableControlsDTO); // DatabaseManagementDefinations -> TableControlsDatabaseListItemDto
		}).ToList();

		var results = await Task.WhenAll(tasks);

		if (results.Any(result => !result.IsActive))
		{
			throw new Exception("Required tables are missing.");
		}

		return await next();

	}
}
