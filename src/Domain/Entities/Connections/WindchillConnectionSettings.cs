using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Connections;

public class WindchillConnectionSettings
{
	public string WindchillServer { get; set; }
	public string WindchillUsername { get; set; }
	public string WindchillPassword { get; set; }
}