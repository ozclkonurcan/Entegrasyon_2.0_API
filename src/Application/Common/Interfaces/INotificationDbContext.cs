using Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Common.Interfaces;

public interface INotificationDbContext
{
	DbSet<ErrorNotification> ErrorNotifications { get; set; }
	DbSet<NotificationHistory> NotificationHistory { get; set; }
	Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
