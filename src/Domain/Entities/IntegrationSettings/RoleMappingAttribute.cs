using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities.IntegrationSettings;

public class RoleMappingAttribute
{
	public int Id { get; set; }
	public int RoleMappingId { get; set; }
	public string AttributeName { get; set; }
	public bool IsSelected { get; set; }

	[JsonIgnore]
	public RoleMapping RoleMapping { get; set; }
}

public class RoleAttributeViewModel
{
	public int Id { get; set; }
	public int RoleMappingId { get; set; }
	public string AttributeName { get; set; }
	public bool IsSelected { get; set; }
}
