using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.ConnectionModule;

public interface IConnectionService
{
	Task<ConnectionSettings> GetConnectionInformation();
	Task<ConnectionSettings> UpdateConnectionInformation(ConnectionSettings connectionSettings);
	Task<bool> ConnectionControl();
	Task<bool> ConnectionControlWithModel(ConnectionSettings model);
}
