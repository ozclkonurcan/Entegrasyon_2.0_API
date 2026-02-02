using Application.Interfaces.EntegrasyonModulu.EMPDocumentServices;
using Application.Interfaces.Generic;
using Domain.Entities;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories.EntegrasyonModulu.EPMDocumentRepositories;

public class EPMDocumentStateRepository : IEPMDocumentStateService
{
	private readonly IGenericRepository<EPMDocument_CANCELLED> _cancelledRepository;
	private readonly IGenericRepository<EPMDocument_RELEASED> _releasedRepository;
	private readonly BaseDbContexts _context;
	private readonly IServiceProvider _serviceProvider;
	private const string FilePath = "deneme.json";

	public EPMDocumentStateRepository(IGenericRepository<EPMDocument_CANCELLED> cancelledRepository, IGenericRepository<EPMDocument_RELEASED> releasedRepository, BaseDbContexts context, IServiceProvider serviceProvider)
	{
		_cancelledRepository = cancelledRepository;
		_releasedRepository = releasedRepository;
		_context = context;
		_serviceProvider = serviceProvider;
	}

	public async Task<EPMDocument_CANCELLED> CANCELLED(CancellationToken token)
	{
		EPMDocument_CANCELLED response = null;
		try
		{
			response = await _cancelledRepository.GetAsync(predicate:x => x.StateDegeri == "CANCELLED");
			if (response != null)
			{
				Console.WriteLine($"RELEASED parça ERP'ye aktarildi : {response.name}");
			}
		}
		catch (Exception ex)
		{
			// Hata loglama işlemleri
		}
		finally
		{
			await Task.Delay(1000, token);
		}
		return response;
	}

	public async Task<EPMDocument_RELEASED> RELEASED(CancellationToken token)
	{
		EPMDocument_RELEASED response = null;
		try
		{
			response = await _releasedRepository.GetAsync(predicate: x => x.StateDegeri == "RELEASED");
			if (response != null)
			{
				Console.WriteLine($"RELEASED parça ERP'ye aktarildi : {response.name}");
			}
		}
		catch (Exception ex)
		{
			// Hata loglama işlemleri
		}
		finally
		{
			await Task.Delay(1000, token);
		}
		return response;
	}
}
