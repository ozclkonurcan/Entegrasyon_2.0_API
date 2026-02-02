using Application.Interfaces.ConnectionModule;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Sql.Queries.SqlContorlsWithModel;

public class ConnectionControlWithModelQuery : IRequest<bool>
{
	public string Server { get; set; }
	public string Database { get; set; }
	public string Username { get; set; }
	public string Password { get; set; }
	public string Schema { get; set; }

	public class ConnectionControlWithModelQueryHandler : IRequestHandler<ConnectionControlWithModelQuery, bool>
	{
		private readonly IConnectionService _connectionService;

		public ConnectionControlWithModelQueryHandler(IConnectionService connectionService)
		{
			_connectionService = connectionService;
		}

		public async Task<bool> Handle(ConnectionControlWithModelQuery request, CancellationToken cancellationToken)
		{
			ConnectionSettings connectionSettings = new ConnectionSettings { 
				Server = request.Server,
				Database = request.Database,
				Username = request.Username,
				Password = request.Password,
				Schema = request.Schema
			};

			return await _connectionService.ConnectionControlWithModel(connectionSettings);
		}
	}
}
