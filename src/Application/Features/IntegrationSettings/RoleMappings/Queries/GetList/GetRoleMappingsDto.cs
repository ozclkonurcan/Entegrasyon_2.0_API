using Domain.Entities.IntegrationSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleMappings.Queries.GetList;

public class GetRoleMappingsDto
{
	public int Id { get; set; }
	public string RoleName { get; set; }
	public int ProcessTagID { get; set; }
	public string SourceApi { get; set; }
	public bool IsActive { get; set; }
	public List<RoleMappingEndpointDto> Endpoints { get; set; }
	public List<RoleMappingAttribute> WindchillAttributes { get; set; }
}

public class RoleMappingEndpointDto
{
	public int Id { get; set; }
	public string TargetApi { get; set; }
	public string Endpoint { get; set; }
	public bool IsActive { get; set; }
}
