using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.IntegrationSettings;

public class RoleProcessTag
{
	public int ProcessTagID { get; set; }
	public string TagName { get; set; }

	// RoleMapping ile bire çok ilişki
	public ICollection<RoleMapping> RoleMappings { get; set; } = new List<RoleMapping>();
}