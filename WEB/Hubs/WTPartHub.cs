using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace WEB.Hubs
{
	public class WTPartHub : Hub
	{
		public override async Task OnConnectedAsync()
		{
			Console.WriteLine($"Yeni bağlantı: {Context.ConnectionId}");
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			Console.WriteLine($"Bağlantı sonlandırıldı: {Context.ConnectionId}");
			await base.OnDisconnectedAsync(exception);
		}

		// Eğer ihtiyacınız varsa örnek bir metot:
		public async Task SendMessage(string user, string message)
		{
			await Clients.All.SendAsync("ReceiveMessage", user, message);
		}

		// Alternate Link güncellemeleri için metod
		public async Task SendAlternateLinkUpdates(
			int notSentCount,
			int sentCount,
			int totalCount,
			int removedNotSentCount,
			int removedSentCount,
			int removedTotalCount)
		{
			await Clients.All.SendAsync(
				"ReceiveAlternateLinkUpdates",
				notSentCount,
				sentCount,
				totalCount,
				removedNotSentCount,
				removedSentCount,
				removedTotalCount);
		}
	}
}