using Domain.Entities;
using Domain.Entities.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.ConnectionModule.WindchillConnectionModule;

public interface IWindchillConnectionService
{
	Task<WindchillConnectionSettings> GetConnectionInformation();
	Task<WindchillConnectionSettings> UpdateConnectionInformation(WindchillConnectionSettings connectionSettings);
	Task<bool> CheckWindchillConnectionInformation(WindchillConnectionSettings connectionSettings);
}
