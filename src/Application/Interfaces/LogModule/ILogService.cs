using Domain.Entities.LogSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.LogModule;

public interface ILogService
{
	Task<List<LogEntry>> GetLogsByDateAsync(DateTime? date,string? level,string? kullaniciAdi);
}
