using Application.Features.Connection.Sql.Constants;
using Application.Interfaces.ConnectionModule;
using CrossCuttingConcerns.ExceptionHandling.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Sql.Rules;

public class ConnectionBusinessRules : BaseBusinessRules
{
	private readonly IConnectionService _connectionService;

	public ConnectionBusinessRules(IConnectionService connectionService)
	{
		_connectionService = connectionService;
	}

	public async Task ConnectionServerCannotBeNullOrEmptyWhenInserted(string Server, string Database, string Username, string Password, string Schema)
	{
		if (string.IsNullOrWhiteSpace(Server))
			throw new BusinessException(ConnectionsMessages.ConnectionServerExists);

		if (string.IsNullOrWhiteSpace(Database))
			throw new BusinessException(ConnectionsMessages.ConnectionDatabaseExists);

		if (string.IsNullOrWhiteSpace(Username))
			throw new BusinessException(ConnectionsMessages.ConnectionUsernameExists);

		if (string.IsNullOrWhiteSpace(Password))
			throw new BusinessException(ConnectionsMessages.ConnectionPasswordExists);

		if (string.IsNullOrWhiteSpace(Schema))
			throw new BusinessException(ConnectionsMessages.ConnectionSchemaExists);


		// Eğer ek bir kontrol yapılacaksa (örneğin, veritabanında aynı kayıt var mı?), burada yapılabilir.

		var existingConnection = await _connectionService.GetConnectionInformation();

		if (existingConnection != null &&
			existingConnection.Server == Server &&
			existingConnection.Database == Database &&
			existingConnection.Username == Username &&
			existingConnection.Password == Password &&
			existingConnection.Schema == Schema)
		{
			throw new BusinessException(ConnectionsMessages.ConnectionAlreadyExists);


		}
	}
}
