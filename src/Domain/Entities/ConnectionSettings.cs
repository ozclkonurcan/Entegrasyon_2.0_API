using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class ConnectionSettings
{
	public string Server { get; set; }
	public string Database { get; set; }
	public string Username { get; set; }
	public string Password { get; set; }
	public string FullURL { get; set; }
	public string Schema { get; set; }
}
