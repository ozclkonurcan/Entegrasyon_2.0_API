using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.WindchillEntities;

public class WTUsers
{
	public string ID { get; set; }
	public string Name { get; set; }
	public string EMail { get; set; }
	public string FullName { get; set; }

	public WTUsers()
	{
	}

	public WTUsers(string id, string name, string email, string fullName)
	{
		ID = id;
		Name = name;
		EMail = email;
		FullName = fullName;
	}
}