using Application.Common.Interfaces;
using Application.Interfaces.Notification;
using Domain.Entities.Notification;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Pipelines.Notification;

public class ErrorNotificationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>, IErrorNotificationRequest
{
	private readonly INotificationService _notificationService;
	private readonly INotificationDbContext _dbContext;
	private readonly ILogger<ErrorNotificationBehavior<TRequest, TResponse>> _logger;
	private readonly IConfiguration _configuration;

	public ErrorNotificationBehavior(
		INotificationService notificationService,
		INotificationDbContext dbContext,
		ILogger<ErrorNotificationBehavior<TRequest, TResponse>> logger,
		IConfiguration configuration)
	{
		_notificationService = notificationService;
		_dbContext = dbContext;
		_logger = logger;
		_configuration = configuration;
	}
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		try
		{
			return await next();
		}
		catch (Exception ex)
		{
			await HandleErrorAsync(ex, request, cancellationToken);
			throw;
		}
	}

	private async Task HandleErrorAsync(Exception exception, TRequest request, CancellationToken cancellationToken)
	{
		var errorKey = $"{exception.GetType().Name}_{request.OperationType}";
		var now = DateTime.UtcNow;

		var errorNotification = await _dbContext.ErrorNotifications
			.FirstOrDefaultAsync(e => e.ErrorKey == errorKey && e.IsActive, cancellationToken);

		if (errorNotification == null)
		{
			errorNotification = new ErrorNotification
			{
				ErrorKey = errorKey,
				ErrorType = exception.GetType().Name,
				ErrorMessage = exception.Message,
				OperationType = request.OperationType,
				FirstOccurrence = now,
				LastOccurrence = now,
				ErrorCount = 1,
				IsActive = true,
				CreatedDate = now,
				UpdatedDate = now
			};
			_dbContext.ErrorNotifications.Add(errorNotification);
		}
		else
		{
			errorNotification.ErrorCount++;
			errorNotification.LastOccurrence = now;
			errorNotification.UpdatedDate = now;
		}

		if (await _notificationService.ShouldSendNotificationAsync(errorKey, cancellationToken))
		{
			var recipients = _configuration.GetSection("ErrorNotification:Recipients").Get<string[]>();
			var subject = $"Windchill Entegrasyon Hatası - {request.OperationType}";
			var content = CreateErrorContent(errorNotification);

			try
			{
				await _notificationService.SendNotificationAsync(subject, content, recipients, cancellationToken);

				var notificationHistory = new NotificationHistory
				{
					ErrorNotificationId = errorNotification.Id,
					Recipients = string.Join(";", recipients),
					Subject = subject,
					Content = content,
					SentTime = now,
					IsSuccess = true
				};
				_dbContext.NotificationHistory.Add(notificationHistory);

				errorNotification.LastNotificationTime = now;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error sending notification for {OperationType}", request.OperationType);
			}
		}

		await _dbContext.SaveChangesAsync(cancellationToken);
	}

	private string CreateErrorContent(ErrorNotification error)
	{
		return $@"Sayın İlgili,

Windchill entegrasyonunda aşağıdaki hata tespit edildi:

İşlem Türü: {error.OperationType}
Hata Türü: {error.ErrorType}
Hata Mesajı: {error.ErrorMessage}
İlk Oluşma Zamanı: {error.FirstOccurrence}
Son Oluşma Zamanı: {error.LastOccurrence}
Toplam Hata Sayısı: {error.ErrorCount}

Hatalı parçalar error tablosuna aktarılmıştır ve saat 20:00 - 24:00 arasında tekrar gönderilmeye çalışılacaktır.

Bu bir otomatik bilgilendirme mesajıdır.

Saygılarımızla,
Windchill Entegrasyon Sistemi";
	}
}