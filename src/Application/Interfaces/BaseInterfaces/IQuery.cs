using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.BaseInterfaces;

public interface IQuery<T>
{
	IQueryable<T> Query();
}
