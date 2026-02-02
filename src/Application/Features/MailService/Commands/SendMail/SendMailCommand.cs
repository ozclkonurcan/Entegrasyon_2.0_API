using Application.Features.MailService.Queries.GetMailSettings;
using Application.Interfaces.Generic;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.MailService.Commands.SendMail;
public class SendMailCommand : IRequest<SendMailResponse>
{
	public string Subject { get; set; }
	public string Body { get; set; }
	public string MailType { get; set; } // "Success", "Error", "FinalFailure"
	public string RelatedEntityType { get; set; }
	public long? RelatedEntityId { get; set; }

	public class SendMailHandler : IRequestHandler<SendMailCommand, SendMailResponse>
	{
		private readonly IMediator _mediator;
		//private readonly IGenericRepository<MailLogs> _mailLogsRepository;
		private readonly ILogger<SendMailHandler> _logger;

		public SendMailHandler(
			IMediator mediator,
			//IGenericRepository<MailLogs> mailLogsRepository,
			ILogger<SendMailHandler> logger)
		{
			_mediator = mediator;
			//_mailLogsRepository = mailLogsRepository;
			_logger = logger;
		}

		public async Task<SendMailResponse> Handle(SendMailCommand request, CancellationToken cancellationToken)
		{
			try
			{
				// Mail ayarlarını al
				var mailSettings = await _mediator.Send(new GetMailSettingsQuery(), cancellationToken);

				if (!mailSettings.Success)
				{
					return new SendMailResponse
					{
						Success = false,
						Message = "Mail ayarları bulunamadı."
					};
				}

				var successCount = 0;
				var errorCount = 0;
				var errors = new List<string>();

				// Her alıcıya mail gönder
				foreach (var recipient in mailSettings.Recipients)
				{
					try
					{
						await SendSingleMail(mailSettings, recipient.EmailAddress, request.Subject, request.Body);

						// Başarılı log
						//await LogMail(mailSettings.Id, recipient.EmailAddress, request, true, null);
						successCount++;

						_logger.LogDebug("Mail gönderildi: {Email}", recipient.EmailAddress);
					}
					catch (Exception ex)
					{
						// Hatalı log
						//await LogMail(mailSettings.Id, recipient.EmailAddress, request, false, ex.Message);
						errorCount++;
						errors.Add($"{recipient.EmailAddress}: {ex.Message}");

						_logger.LogError(ex, "Mail gönderilemedi: {Email}", recipient.EmailAddress);
					}
				}

				return new SendMailResponse
				{
					Success = successCount > 0,
					Message = $"Başarılı: {successCount}, Hatalı: {errorCount}",
					SuccessCount = successCount,
					ErrorCount = errorCount,
					Errors = errors
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Mail gönderimi sırasında genel hata oluştu.");

				return new SendMailResponse
				{
					Success = false,
					Message = $"Mail gönderimi başarısız: {ex.Message}"
				};
			}
		}

		private async Task SendSingleMail(GetMailSettingsDto settings, string toEmail, string subject, string body)
		{
			using var client = new SmtpClient(settings.SmtpServer, settings.SmtpPort)
			{
				Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword),
				EnableSsl = settings.EnableSsl
			};

			var message = new MailMessage
			{
				From = new MailAddress(settings.FromEmail, settings.FromDisplayName),
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			};

			message.To.Add(toEmail);

			await client.SendMailAsync(message);
		}

		//private async Task LogMail(long mailSettingsId, string toEmail, SendMailCommand request, bool isSuccess, string errorMessage)
		//{
		//	var log = new MailLogs
		//	{
		//		MailSettingsId = mailSettingsId,
		//		ToEmail = toEmail,
		//		Subject = request.Subject,
		//		Body = request.Body,
		//		IsSuccess = isSuccess,
		//		ErrorMessage = errorMessage,
		//		MailType = request.MailType,
		//		RelatedEntityId = request.RelatedEntityId,
		//		RelatedEntityType = request.RelatedEntityType,
		//		SentDate = DateTime.Now,
		//		IsActive = true,
		//		CreatedDate = DateTime.Now
		//	};

		//	await _mailLogsRepository.AddAsync(log);
		//}
	}
}