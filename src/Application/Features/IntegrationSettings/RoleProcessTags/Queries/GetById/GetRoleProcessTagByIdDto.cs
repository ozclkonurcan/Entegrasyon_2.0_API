using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleProcessTags.Queries.GetById;

   public class GetRoleProcessTagByIdDto
    {
	public int ProcessTagID { get; set; }
	public string TagName { get; set; }
}
