using Application.Features.DatabaseManagement.Rules;
using Application.Interfaces.DatabaseManagementModule;
using AutoMapper;
using Domain.Entities.DatabaseManagement;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DatabaseManagement.Commands.Create;

public class CreateDatabaseCommand : IRequest<CreatedDatabaseResponse>
{
	public class CreateDatabaseCommandHandler : IRequestHandler<CreateDatabaseCommand, CreatedDatabaseResponse>
	{
		private readonly IDatabaseManagementService _databaseManagementService;
		private readonly IMapper _mapper;
		private readonly DatabaseManagementBusinessRules _businessRules;

		public CreateDatabaseCommandHandler(IDatabaseManagementService databaseManagementService, IMapper mapper, DatabaseManagementBusinessRules businessRules)
		{
			_databaseManagementService = databaseManagementService;
			_mapper = mapper;
			_businessRules = businessRules;
		}

		public async Task<CreatedDatabaseResponse> Handle(CreateDatabaseCommand request, CancellationToken cancellationToken)
		{
			var createdTable = await _databaseManagementService.SetupTablels();
			CreatedDatabaseResponse createdDatabaseResponse = _mapper.Map<CreatedDatabaseResponse>(createdTable);
			return createdDatabaseResponse;
		}
	}
}
