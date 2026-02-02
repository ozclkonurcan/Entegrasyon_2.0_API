using Application.Features.MailService.Commands.SendMail;
using Application.Features.MailService.Queries.GetMailSettings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Pipelines.MailNotification;

public class MailNotificationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>, IMailNotifiableRequest
{
	private readonly ILogger<MailNotificationBehavior<TRequest, TResponse>> _logger;
	private readonly IServiceProvider _serviceProvider;

	public MailNotificationBehavior(
		ILogger<MailNotificationBehavior<TRequest, TResponse>> logger,
		IServiceProvider serviceProvider)
	{
		_logger = logger;
		_serviceProvider = serviceProvider;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		TResponse response;
		bool isSuccess = false;
		string errorMessage = null;

		try
		{
			// Ana işlemi çalıştır
			response = await next();

			// Response'dan başarı durumunu kontrol et
			isSuccess = CheckIfSuccessful(response);

			_logger.LogDebug("İşlem tamamlandı: {RequestType}, Başarılı: {IsSuccess}",
				typeof(TRequest).Name, isSuccess);
		}
		catch (Exception ex)
		{
			isSuccess = false;
			errorMessage = ex.Message;

			_logger.LogError(ex, "İşlem başarısız: {RequestType}", typeof(TRequest).Name);

			// Exception'ı tekrar fırlat
			throw;
		}
		finally
		{
			// ✅ Mail gönderme işlemini tamamen ayrı bir task'ta yap
			// Transaction tamamlandıktan sonra çalışacak
			_ = Task.Run(async () =>
			{
				// Kısa bir bekleme - transaction'ın tamamlanması için
				await Task.Delay(100);

				try
				{
					await SendMailNotificationAsync(request, isSuccess, errorMessage, CancellationToken.None);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Mail bildirimi gönderilirken hata oluştu: {RequestType}", typeof(TRequest).Name);
				}
			});
		}

		return response;
	}

	private bool CheckIfSuccessful(TResponse response)
	{
		// Response'da Success property'si varsa kontrol et
		var successProperty = response?.GetType().GetProperty("Success");
		if (successProperty != null && successProperty.PropertyType == typeof(bool))
		{
			return (bool)successProperty.GetValue(response);
		}

		// Default olarak exception yoksa başarılı kabul et
		return true;
	}

	private async Task SendMailNotificationAsync(TRequest request, bool isSuccess, string errorMessage, CancellationToken cancellationToken)
	{
		try
		{
			// ✅ Yeni scope oluştur - transaction'dan bağımsız
			using var scope = _serviceProvider.CreateScope();
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

			// Mail ayarlarını kontrol et
			var mailSettings = await mediator.Send(new GetMailSettingsQuery(), cancellationToken);

			if (!mailSettings.Success || mailSettings.Recipients?.Any() != true)
			{
				_logger.LogDebug("Mail ayarları bulunamadı veya alıcı yok, mail gönderilmiyor.");
				return;
			}

			// Gönderim koşullarını kontrol et
			bool shouldSend = isSuccess ? request.SendOnSuccess : request.SendOnError;

			if (!shouldSend)
			{
				_logger.LogDebug("Mail gönderim koşulu sağlanmadı. Başarılı: {IsSuccess}, SendOnSuccess: {SendOnSuccess}, SendOnError: {SendOnError}",
					isSuccess, request.SendOnSuccess, request.SendOnError);
				return;
			}

			// Mail içeriğini hazırla
			var subject = request.GetMailSubject();
			var body = request.GetMailBody(isSuccess, errorMessage);

			// Mail gönder
			var sendMailCommand = new SendMailCommand
			{
				Subject = subject,
				Body = body,
				MailType = isSuccess ? "Success" : "Error",
				RelatedEntityType = request.GetEntityType(),
				RelatedEntityId = request.GetEntityId()
			};

			await mediator.Send(sendMailCommand, cancellationToken);

			_logger.LogInformation("Mail bildirimi gönderildi: {RequestType}, Başarılı: {IsSuccess}",
				typeof(TRequest).Name, isSuccess);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Mail bildirimi gönderilirken hata oluştu: {RequestType}", typeof(TRequest).Name);
		}
	}
}