using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Windchill.Queries.GetList;

public class GetListWindchillConnectionListItemDto
{


	public string WindchillServer { get; set; }
	public string WindchillUsername { get; set; }
	public string WindchillPassword { get; set; }


	public GetListWindchillConnectionListItemDto()
	{
		WindchillServer = string.Empty;
		WindchillUsername = string.Empty;
		WindchillPassword = string.Empty;
	}

	public GetListWindchillConnectionListItemDto(string windchillServer, string windchillUsername, string windchillPassword)
	{
		WindchillServer = windchillServer;
		WindchillUsername = windchillUsername;
		WindchillPassword = windchillPassword;
	}
}
