using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.BaseInterfaces.Dynamic;

public class Sort
{
	public string Field { get; set; }
	public string Dir { get; set; }


	public Sort()
	{
		Field = string.Empty;
		Dir = string.Empty;
	}

	public Sort(string filed, string dir)
	{
		Field = filed;
		Dir = dir;
	}
}
