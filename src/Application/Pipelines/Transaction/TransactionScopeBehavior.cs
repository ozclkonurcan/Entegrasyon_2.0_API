using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Pipelines.Transaction;

public class TransactionScopeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>, ITransactionalRequest
{
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		// Distributed transaction desteğini etkinleştir
		TransactionManager.ImplicitDistributedTransactions = true;

		var options = new TransactionOptions
		{
			IsolationLevel = IsolationLevel.ReadCommitted,
			Timeout = TimeSpan.FromMinutes(10) // Zaman aşımı süresini 10 dakikaya çıkarın
		};

		using TransactionScope transactionScope = new(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);
		TResponse response;
		try
		{
			response = await next();
			transactionScope.Complete();
		}
		catch (Exception)
		{
			transactionScope.Dispose();
			throw;
		}
		return response;
	}
}


//using MediatR;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Transactions;

//namespace Application.Pipelines.Transaction;

//public class TransactionScopeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
//where TRequest : IRequest<TResponse>, ITransactionalRequest
//{
//	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
//	{

//		var options = new TransactionOptions
//		{
//			IsolationLevel = IsolationLevel.ReadCommitted,
//			Timeout = TimeSpan.FromMinutes(10) // Zaman aşımı süresini 10 dakikaya çıkarın
//		};

//		using TransactionScope transactionScope = new(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);
//		TResponse response;
//		try
//		{
//			response = await next();
//			transactionScope.Complete();
//		}
//		catch (Exception)
//		{
//			transactionScope.Dispose();
//			throw;
//		}
//		return response;
//	}
//}
