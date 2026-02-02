using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.EntegrasyonModulu.WTPartServices;

public interface IStateService
{
	Task<WTPart> RELEASED(CancellationToken token);
	Task<WTPartError> ERRORRELEASED(CancellationToken token);
	Task<WTPart> CANCELLED(CancellationToken token);
	Task<WTPartError> ERRORCANCELLED(CancellationToken token);
	Task INWORK(CancellationToken token);

}
