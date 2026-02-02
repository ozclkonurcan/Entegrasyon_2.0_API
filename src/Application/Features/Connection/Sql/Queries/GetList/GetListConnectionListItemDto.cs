using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Sql.Queries.GetList;

public class GetListConnectionListItemDto
{

	public string FullURL { get; set; }
	public string Server { get; set; }
	public string Database { get; set; }
	public string Schema { get; set; }

	public GetListConnectionListItemDto()
	{

		FullURL = string.Empty;
		Server = string.Empty;
		Database = string.Empty;
		Schema = string.Empty;
	}
}
