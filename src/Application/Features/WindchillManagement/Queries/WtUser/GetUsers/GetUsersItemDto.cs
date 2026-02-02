using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillManagement.Queries.WtUser.GetUsers;

public class GetUsersItemDto
{
	public string? Name { get; set; }
	public string? EMail { get; set; }
	public string? FullName { get; set; }
}
