using Application.Features.Connection.Sql.Commands.Update;
using Application.Features.Connection.Sql.Rules;
using Application.Features.Connection.Windchill.Rules;
using Application.Interfaces.ConnectionModule;
using Application.Interfaces.ConnectionModule.WindchillConnectionModule;
using Application.Pipelines.Logging;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Connections;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Windchill.Commands.Update;

public class UpdateWindchillConnectionCommand : IRequest<UpdatedWindchillConnectionResponse>,ILoggableRequest
{
	public string WindchillServer { get; set; }
	public string WindchillUsername { get; set; }
	public string WindchillPassword { get; set; }

	public string? LogMessage { get; set; }

	public class UpdateWindchillConnectionCommandHandler : IRequestHandler<UpdateWindchillConnectionCommand, UpdatedWindchillConnectionResponse>
	{
		private readonly IWindchillConnectionService _connectionService;
		private readonly IMapper _mapper;
		private readonly WindchillConnectionBusinessRules _businessRules;

		public UpdateWindchillConnectionCommandHandler(IWindchillConnectionService connectionService, IMapper mapper, WindchillConnectionBusinessRules businessRules)
		{
			_connectionService = connectionService;
			_mapper = mapper;
			_businessRules = businessRules;
		}


	
		public async Task<UpdatedWindchillConnectionResponse> Handle(UpdateWindchillConnectionCommand request, CancellationToken cancellationToken)
		{
			WindchillConnectionSettings connectionSettings = await _connectionService.GetConnectionInformation();
			connectionSettings = _mapper.Map(request, connectionSettings);

			await _connectionService.UpdateConnectionInformation(connectionSettings);

			UpdatedWindchillConnectionResponse response = _mapper.Map<UpdatedWindchillConnectionResponse>(connectionSettings);
			request.LogMessage = "Windchill baglanti ayarlari yapildi";
			return response;

		}
	}
}
