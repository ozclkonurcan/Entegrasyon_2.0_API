using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Connection.Api.Queries
{
	public class CheckApiConnectionQuery : IRequest<bool>
	{
		// Bu sorgu için herhangi bir parametreye gerek yok
	}

	public class CheckApiConnectionQueryHandler : IRequestHandler<CheckApiConnectionQuery, bool>
	{
		public Task<bool> Handle(CheckApiConnectionQuery request, CancellationToken cancellationToken)
		{
			// API çalışıyorsa true döner
			return Task.FromResult(true);
		}
	}
}