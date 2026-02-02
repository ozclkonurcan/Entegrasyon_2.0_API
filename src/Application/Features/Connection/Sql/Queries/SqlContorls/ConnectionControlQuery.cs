using Application.Interfaces.ConnectionModule;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Connection.Sql.Queries.SqlContorls;

public class ConnectionControlQuery : IRequest<bool>
{
	public class ConnectionControlQueryHandler : IRequestHandler<ConnectionControlQuery, bool>
	{
		private readonly IConnectionService _connectionService;

		public ConnectionControlQueryHandler(IConnectionService connectionService)
		{
			_connectionService = connectionService;
		}

		public async Task<bool> Handle(ConnectionControlQuery request, CancellationToken cancellationToken)
		{
			// ConnectionControl metodunu çağır ve bool değerini döndür
			return await _connectionService.ConnectionControl();
		}
	}
}