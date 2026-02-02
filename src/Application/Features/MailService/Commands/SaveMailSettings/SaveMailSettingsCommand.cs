using Application.Interfaces.Generic;
using Domain.Entities.MailService;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.MailService.Commands.SaveMailSettings;

public class SaveMailSettingsCommand : IRequest<SaveMailSettingsResponse>
{
	public string SmtpServer { get; set; }
	public int SmtpPort { get; set; }
	public string SmtpUsername { get; set; }
	public string SmtpPassword { get; set; }
	public bool EnableSsl { get; set; }
	public string FromEmail { get; set; }
	public string FromDisplayName { get; set; }

	public bool SendOnError { get; set; }
	public bool SendOnSuccess { get; set; }
	public bool SendOnFinalFailure { get; set; }

	public List<MailRecipientRequest> Recipients { get; set; } = new List<MailRecipientRequest>();

	public class SaveMailSettingsCommandHandler : IRequestHandler<SaveMailSettingsCommand, SaveMailSettingsResponse>
	{
		private readonly IGenericRepository<MailSettings> _mailSettingsRepository;
		private readonly IGenericRepository<MailRecipient> _mailRecipientRepository;

		public SaveMailSettingsCommandHandler(
			IGenericRepository<MailSettings> mailSettingsRepository,
			IGenericRepository<MailRecipient> mailRecipientRepository)
		{
			_mailSettingsRepository = mailSettingsRepository;
			_mailRecipientRepository = mailRecipientRepository;
		}

		public async Task<SaveMailSettingsResponse> Handle(SaveMailSettingsCommand request, CancellationToken cancellationToken)
		{
			try
			{
				// Mevcut ayar var mı kontrol et
				var existingSettings = await _mailSettingsRepository.GetFirstAsync(
					include: x => x.Include(m => m.MailRecipients),
					enableTracking: true,
					cancellationToken: cancellationToken
				);

				if (existingSettings != null)
				{
					// Güncelleme
					existingSettings.SmtpServer = request.SmtpServer;
					existingSettings.SmtpPort = request.SmtpPort;
					existingSettings.SmtpUsername = request.SmtpUsername;
					existingSettings.SmtpPassword = request.SmtpPassword;
					existingSettings.EnableSsl = request.EnableSsl;
					existingSettings.FromEmail = request.FromEmail;
					existingSettings.FromDisplayName = request.FromDisplayName;
					existingSettings.SendOnError = request.SendOnError;
					existingSettings.SendOnSuccess = request.SendOnSuccess;
					existingSettings.SendOnFinalFailure = request.SendOnFinalFailure;
					existingSettings.UpdatedDate = DateTime.Now;

					// Mevcut alıcıları sil
					if (existingSettings.MailRecipients.Any())
					{
						await _mailRecipientRepository.DeleteRangeAsync(existingSettings.MailRecipients.ToList(), permanent: true);
					}

					// Yeni alıcıları ekle
					foreach (var recipient in request.Recipients)
					{
						var mailRecipient = new MailRecipient
						{
							MailSettingsId = existingSettings.Id,
							EmailAddress = recipient.EmailAddress,
							DisplayName = recipient.DisplayName,
							IsActive = true,
							CreatedDate = DateTime.Now
						};
						await _mailRecipientRepository.AddAsync(mailRecipient);
					}

					await _mailSettingsRepository.UpdateAsync(existingSettings);

					return new SaveMailSettingsResponse
					{
						Success = true,
						Message = "Mail ayarları başarıyla güncellendi.",
						MailSettingsId = existingSettings.Id
					};
				}
				else
				{
					// Yeni kayıt
					var newSettings = new MailSettings
					{
						SmtpServer = request.SmtpServer,
						SmtpPort = request.SmtpPort,
						SmtpUsername = request.SmtpUsername,
						SmtpPassword = request.SmtpPassword,
						EnableSsl = request.EnableSsl,
						FromEmail = request.FromEmail,
						FromDisplayName = request.FromDisplayName,
						SendOnError = request.SendOnError,
						SendOnSuccess = request.SendOnSuccess,
						SendOnFinalFailure = request.SendOnFinalFailure,
						IsActive = true,
						CreatedDate = DateTime.Now
					};

					var savedSettings = await _mailSettingsRepository.AddAsync(newSettings);

					// Alıcıları ekle
					foreach (var recipient in request.Recipients)
					{
						var mailRecipient = new MailRecipient
						{
							MailSettingsId = savedSettings.Id,
							EmailAddress = recipient.EmailAddress,
							DisplayName = recipient.DisplayName,
							IsActive = true,
							CreatedDate = DateTime.Now
						};
						await _mailRecipientRepository.AddAsync(mailRecipient);
					}

					return new SaveMailSettingsResponse
					{
						Success = true,
						Message = "Mail ayarları başarıyla kaydedildi.",
						MailSettingsId = savedSettings.Id
					};
				}
			}
			catch (Exception ex)
			{
				return new SaveMailSettingsResponse
				{
					Success = false,
					Message = $"Mail ayarları kaydedilirken hata oluştu: {ex.Message}"
				};
			}
		}
	}
}