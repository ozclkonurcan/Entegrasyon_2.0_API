using WEB.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace WEB.Services;

public class WTPartDataBroadcastService: BackgroundService
{
	private readonly IHubContext<WTPartHub> _hubContext;
	private readonly IMediator _mediator;
	private readonly ILogger<WTPartDataBroadcastService> _logger;
	private readonly TimeSpan _broadcastInterval = TimeSpan.FromSeconds(10); // örneğin her 10 saniyede bir

	public WTPartDataBroadcastService(
		IHubContext<WTPartHub> hubContext,
		IMediator mediator,
		ILogger<WTPartDataBroadcastService> logger)
	{
		_hubContext = hubContext;
		_mediator = mediator;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		
	}
}
