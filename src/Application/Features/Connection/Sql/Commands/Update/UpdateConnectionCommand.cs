using Application.Features.Connection.Sql.Rules;
using Application.Interfaces.ConnectionModule;
using Application.Pipelines.Logging;
using AutoMapper;
using CrossCuttingConcerns.ExceptionHandling.Types;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Sql.Commands.Update;

public class UpdateConnectionCommand : IRequest<UpdatedConnectionResponse> , ILoggableRequest
{
	public string Server { get; set; }
	public string Database { get; set; }
	public string Username { get; set; }
	public string Password { get; set; }
	public string Schema { get; set; }

	public string? LogMessage {get;set;}

	public class UpdateConnectionCommandHandler : IRequestHandler<UpdateConnectionCommand, UpdatedConnectionResponse>
	{
		private readonly IConnectionService _connectionService;
		private readonly IMapper _mapper;
		private readonly ConnectionBusinessRules _connectionBusinessRules;

		public UpdateConnectionCommandHandler(IConnectionService connectionService, IMapper mapper, ConnectionBusinessRules connectionBusinessRules)
		{
			_connectionService = connectionService;
			_mapper = mapper;
			_connectionBusinessRules = connectionBusinessRules;
		}

		public async Task<UpdatedConnectionResponse> Handle(UpdateConnectionCommand request, CancellationToken cancellationToken)
		{
			await _connectionBusinessRules.ConnectionServerCannotBeNullOrEmptyWhenInserted(request.Server, request.Database, request.Username, request.Password, request.Schema);

			ConnectionSettings connectionSettings = await _connectionService.GetConnectionInformation();
			connectionSettings = _mapper.Map(request, connectionSettings);

			await _connectionService.UpdateConnectionInformation(connectionSettings);

			UpdatedConnectionResponse response = _mapper.Map<UpdatedConnectionResponse>(connectionSettings);
			request.LogMessage = "Sql ayarlari yapildi";
			return response;

		}
	}
}
