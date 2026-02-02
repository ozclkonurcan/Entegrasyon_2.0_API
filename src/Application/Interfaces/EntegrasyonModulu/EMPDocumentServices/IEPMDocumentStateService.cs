using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.EntegrasyonModulu.EMPDocumentServices;

public interface IEPMDocumentStateService
{
	Task<EPMDocument_RELEASED> RELEASED(CancellationToken token);
	Task<EPMDocument_CANCELLED> CANCELLED(CancellationToken token);
	//Task<> ERRORRELEASED(CancellationToken token);
	//Task<> ERRORCANCELLED(CancellationToken token);
	//Task INWORK(CancellationToken token);
}
