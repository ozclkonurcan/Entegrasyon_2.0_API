using Application.Features.MailService.Commands.SendMail;
using Application.Interfaces.Mail;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Mail;

public class MailService : IMailService
{
	private readonly IMediator _mediator;
	private readonly MailTemplateService _templateService;
	private readonly ILogger<MailService> _logger;

	public MailService(IMediator mediator, MailTemplateService templateService, ILogger<MailService> logger)
	{
		_mediator = mediator;
		_templateService = templateService;
		_logger = logger;
	}

	public async Task SendErrorMailAsync(string entityType, string entityNumber, string entityName, string errorMessage, long? entityId = null)
	{
		try
		{
			var subject = _templateService.GetSubject(entityType, false, entityNumber);
			var body = _templateService.GetErrorTemplate(entityType, entityNumber, entityName, errorMessage);

			await _mediator.Send(new SendMailCommand
			{
				Subject = subject,
				Body = body,
				MailType = "Error",
				RelatedEntityType = entityType,
				RelatedEntityId = entityId
			});

			_logger.LogInformation("Hata maili gönderildi: {EntityType} - {EntityNumber}", entityType, entityNumber);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Mail gönderilemedi: {EntityType} - {EntityNumber}", entityType, entityNumber);
		}
	}

	public async Task SendSuccessMailAsync(string entityType, string entityNumber, string entityName, string successMessage, long? entityId = null)
	{
		try
		{
			var subject = _templateService.GetSubject(entityType, true, entityNumber);
			var body = _templateService.GetSuccessTemplate(entityType, entityNumber, entityName, successMessage);

			await _mediator.Send(new SendMailCommand
			{
				Subject = subject,
				Body = body,
				MailType = "Success",
				RelatedEntityType = entityType,
				RelatedEntityId = entityId
			});

			_logger.LogInformation("Başarı maili gönderildi: {EntityType} - {EntityNumber}", entityType, entityNumber);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Mail gönderilemedi: {EntityType} - {EntityNumber}", entityType, entityNumber);
		}
	}

	public async Task SendCustomMailAsync(string subject, string body, string mailType = "Custom", string entityType = null, long? entityId = null)
	{
		try
		{
			await _mediator.Send(new SendMailCommand
			{
				Subject = subject,
				Body = body,
				MailType = mailType,
				RelatedEntityType = entityType,
				RelatedEntityId = entityId
			});

			_logger.LogInformation("Özel mail gönderildi: {Subject}", subject);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Özel mail gönderilemedi: {Subject}", subject);
		}
	}
}