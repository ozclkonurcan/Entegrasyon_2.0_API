using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Windchill.Commands.Update;

public class UpdatedWindchillConnectionResponse
{
	public string WindchillServer { get; set; }
	public string WindchillUsername { get; set; }
	public string WindchillPassword { get; set; }
}
