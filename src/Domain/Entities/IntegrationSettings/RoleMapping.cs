using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.IntegrationSettings;

public class RoleMapping
{
	public int Id { get; set; }
	public string RoleName { get; set; }
	public int ProcessTagID { get; set; }
	public string SourceApi { get; set; }
	public bool IsActive { get; set; }

	public RoleProcessTag RoleProcessTag { get; set; }
	// Hedef API'lerin detaylı bilgisini ayrı satırlarda saklamak için Endpoints koleksiyonu
	public ICollection<RoleMappingEndpoint> Endpoints { get; set; } = new List<RoleMappingEndpoint>();
	public ICollection<RoleMappingAttribute> WindchillAttributes { get; set; } = new List<RoleMappingAttribute>();
}

public class RoleMappingEndpoint
{
	public int Id { get; set; }
	public int RoleMappingId { get; set; }
	public string TargetApi { get; set; }
	public string Endpoint { get; set; }
	public bool IsActive { get; set; }

	// Navigation property
	public RoleMapping RoleMapping { get; set; }
}