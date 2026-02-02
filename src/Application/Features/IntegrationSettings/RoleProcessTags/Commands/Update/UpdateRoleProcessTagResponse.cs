using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleProcessTags.Commands.Update;

public class UpdateRoleProcessTagResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }
	public int ProcessTagID { get; set; }
}