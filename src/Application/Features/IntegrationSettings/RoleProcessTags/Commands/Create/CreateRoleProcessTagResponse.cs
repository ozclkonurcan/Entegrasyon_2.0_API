using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleProcessTags.Commands.Create;

public class CreateRoleProcessTagResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }
	public int ProcessTagID { get; set; }
}
