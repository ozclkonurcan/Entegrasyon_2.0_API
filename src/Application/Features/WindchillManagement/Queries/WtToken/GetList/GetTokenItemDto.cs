using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillManagement.Queries.WtToken.GetList;

public class GetTokenItemDto
{
	public string NonceKey { get; set; }
	public string NonceValue { get; set; }
}
