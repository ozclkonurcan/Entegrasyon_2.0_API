using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Notification;

public interface INotificationService
{
	Task SendNotificationAsync(string subject, string content, string[] recipients, CancellationToken cancellationToken = default);
	Task<bool> ShouldSendNotificationAsync(string errorKey, CancellationToken cancellationToken = default);
}


