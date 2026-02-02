using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.BaseInterfaces.Dynamic;

public class Filter
{
	public string Field { get; set; }
	public string? Value { get; set; }
	public string Operator { get; set; }
	public string? Logic { get; set; }


	public IEnumerable<Filter>? Filters { get; set; }

	public Filter()
	{
		Field = string.Empty;
		Operator = string.Empty;
	}

	public Filter(string filed, string @operator)
	{
		Field = filed;
		Operator = @operator;
	}
}
