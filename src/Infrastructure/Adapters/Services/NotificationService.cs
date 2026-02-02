using Application.Common.Interfaces;
using Application.Interfaces.Notification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Adapters.Services;

public class NotificationService : INotificationService
{
	private readonly INotificationDbContext _dbContext;
	private readonly IEmailService _emailService;
	private readonly ILogger<NotificationService> _logger;
	private static readonly TimeSpan _notificationInterval = TimeSpan.FromHours(3);

	public NotificationService(
		INotificationDbContext dbContext,
		IEmailService emailService,
		ILogger<NotificationService> logger)
	{
		_dbContext = dbContext;
		_emailService = emailService;
		_logger = logger;
	}

	public async Task SendNotificationAsync(string subject, string content, string[] recipients, CancellationToken cancellationToken = default)
	{
		await _emailService.SendEmailAsync(subject, content, recipients, cancellationToken);
	}

	public async Task<bool> ShouldSendNotificationAsync(string errorKey, CancellationToken cancellationToken = default)
	{
		var errorNotification = await _dbContext.ErrorNotifications
			.Where(e => e.ErrorKey == errorKey && e.IsActive)
			.OrderByDescending(e => e.LastNotificationTime)
			.FirstOrDefaultAsync(cancellationToken);

		if (errorNotification == null)
			return true;

		return !errorNotification.LastNotificationTime.HasValue ||
			   (DateTime.UtcNow - errorNotification.LastNotificationTime.Value) >= _notificationInterval;
	}
}
