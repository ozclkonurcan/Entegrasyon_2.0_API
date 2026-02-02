using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Services;

public class BackgroundTaskService
{
	private readonly IDbContextFactory<BaseDbContexts> _dbContextFactory;

	// Constructor Dependency Injection
	public BackgroundTaskService(IDbContextFactory<BaseDbContexts> dbContextFactory)
	{
		_dbContextFactory = dbContextFactory;
	}

	// Genel işlemleri yönetebilecek metod
	public async Task ExecuteTaskAsync(Func<BaseDbContexts, Task> task)
	{
		using (var context = await _dbContextFactory.CreateDbContextAsync())
		{
			// Task'ı BaseDbContexts ile çalıştırıyoruz
			await task(context);
		}
	}
}
