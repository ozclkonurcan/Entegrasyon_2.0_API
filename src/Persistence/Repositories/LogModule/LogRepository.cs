using Application.Interfaces.LogModule;
using Domain.Entities.LogSettings;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories.LogModule;

public class LogRepository : ILogService
{
	private readonly BaseDbContexts _dbContexts;

	public LogRepository(BaseDbContexts dbContexts)
	{
		_dbContexts = dbContexts;
	}

	public async Task<List<LogEntry>> GetLogsByDateAsync(DateTime? date = null, string level = null, string kullaniciAdi = null)
	{
		var query = _dbContexts.Logs.AsQueryable();

		if (date.HasValue)
		{
			query = query.Where(log => log.TimeStamp.Date == date.Value.Date);
		}

		if (!string.IsNullOrEmpty(level))
		{
			query = query.Where(log => log.Level == level);
		}

		if (!string.IsNullOrEmpty(kullaniciAdi))
		{
			query = query.Where(log => log.KullaniciAdi == kullaniciAdi);
		}

		return await query.ToListAsync();
	}
}
