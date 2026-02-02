using Domain.Entities.WindchillEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.WindchillModule;

public interface IWindchillService
{
	Task<WrsToken> GetTokenAsync();
	Task<string> GetUserAsync();
	Task<List<WTUsers>> GetFindUserAsync(string SearchTerm);
}
