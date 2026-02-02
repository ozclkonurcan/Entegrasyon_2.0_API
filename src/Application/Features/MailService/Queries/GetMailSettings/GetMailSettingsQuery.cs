using Application.Features.MailService.Commands.SaveMailSettings;
using Application.Interfaces.Generic;
using Domain.Entities.MailService;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.MailService.Queries.GetMailSettings;


public class GetMailSettingsQuery : IRequest<GetMailSettingsDto>
{
	public class GetMailSettingsHandler : IRequestHandler<GetMailSettingsQuery, GetMailSettingsDto>
	{
		private readonly IGenericRepository<MailSettings> _mailSettingsRepository;

		public GetMailSettingsHandler(IGenericRepository<MailSettings> mailSettingsRepository)
		{
			_mailSettingsRepository = mailSettingsRepository;
		}

		public async Task<GetMailSettingsDto> Handle(GetMailSettingsQuery request, CancellationToken cancellationToken)
		{
			var settings = await _mailSettingsRepository.GetFirstAsync(
				include: x => x.Include(m => m.MailRecipients.Where(r => r.IsActive)),
				enableTracking: false,
				cancellationToken: cancellationToken
			);

			if (settings == null)
			{
				return new GetMailSettingsDto
				{
					Success = false,
					Message = "Mail ayarları bulunamadı."
				};
			}

			return new GetMailSettingsDto
			{
				Success = true,
				Message = "Mail ayarları başarıyla getirildi.",
				Id = settings.Id,
				SmtpServer = settings.SmtpServer,
				SmtpPort = settings.SmtpPort,
				SmtpUsername = settings.SmtpUsername,
				SmtpPassword = settings.SmtpPassword,
				EnableSsl = settings.EnableSsl,
				FromEmail = settings.FromEmail,
				FromDisplayName = settings.FromDisplayName,
				SendOnError = settings.SendOnError,
				SendOnSuccess = settings.SendOnSuccess,
				SendOnFinalFailure = settings.SendOnFinalFailure,
				Recipients = settings.MailRecipients.Select(r => new MailRecipientDto
				{
					Id = r.Id,
					EmailAddress = r.EmailAddress,
					DisplayName = r.DisplayName
				}).ToList()
			};
		}
	}
}