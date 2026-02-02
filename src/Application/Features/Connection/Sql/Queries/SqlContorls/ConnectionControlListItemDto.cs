using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Sql.Queries.SqlContorls;

public class ConnectionControlListItemDto
{
	

	public string FullURL { get; set; }
	public string Server { get; set; }
	public string Database { get; set; }
	public string Schema { get; set; }

	public bool ConnectionStatus { get; set; }

	public ConnectionControlListItemDto(string fullURL, string server, string database, string schema, bool connectionStatus)
	{
		FullURL = fullURL;
		Server = server;
		Database = database;
		Schema = schema;
		ConnectionStatus = connectionStatus;
	}
}
