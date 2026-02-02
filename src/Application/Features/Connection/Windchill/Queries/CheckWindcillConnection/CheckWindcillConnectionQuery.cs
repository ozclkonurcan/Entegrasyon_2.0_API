using Application.Interfaces.ConnectionModule.WindchillConnectionModule;
using Domain.Entities.Connections;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Windchill.Queries.CheckWindcillConnection;

public class CheckWindcillConnectionQuery : IRequest<bool>
{
	public string WindchillServer { get; set; }
	public string WindchillUsername { get; set; }
	public string WindchillPassword { get; set; }
	public class CheckWindcillConnectionQueryHandler : IRequestHandler<CheckWindcillConnectionQuery, bool>
	{
		private readonly IWindchillConnectionService _connectionService;

		public CheckWindcillConnectionQueryHandler(IWindchillConnectionService connectionService)
		{
			_connectionService = connectionService;
		}

		public async Task<bool> Handle(CheckWindcillConnectionQuery request, CancellationToken cancellationToken)
		{
			var connectionSettings = new WindchillConnectionSettings
			{
				WindchillServer = request.WindchillServer,
				WindchillUsername = request.WindchillUsername,
				WindchillPassword = request.WindchillPassword
			};

			// Windchill bağlantısını kontrol et
			return await _connectionService.CheckWindchillConnectionInformation(connectionSettings);
		}
	}
}
