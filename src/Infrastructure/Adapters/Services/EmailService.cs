using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Application.Common.Interfaces;

namespace Infrastructure.Adapters.Services;

public class EmailService : IEmailService
{
	private readonly IConfiguration _configuration;
	private readonly ILogger<EmailService> _logger;

	public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
	{
		_configuration = configuration;
		_logger = logger;
	}

	public async Task SendEmailAsync(string subject, string body, string[] recipients, CancellationToken cancellationToken = default)
	{
		try
		{
			using var message = new MailMessage();
			var smtpClient = new SmtpClient(_configuration["SmtpSettings:Host"])
			{
				Port = int.Parse(_configuration["SmtpSettings:Port"]),
				Credentials = new NetworkCredential(
					_configuration["SmtpSettings:Username"],
					_configuration["SmtpSettings:Password"]),
				EnableSsl = bool.Parse(_configuration["SmtpSettings:EnableSsl"])
			};

			message.From = new MailAddress(_configuration["SmtpSettings:FromEmail"]);
			foreach (var recipient in recipients)
			{
				message.To.Add(recipient);
			}
			message.Subject = subject;
			message.Body = body;
			message.IsBodyHtml = false;

			await smtpClient.SendMailAsync(message, cancellationToken);
			_logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", recipients));
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error sending email to {Recipients}", string.Join(", ", recipients));
			throw;
		}
	}
}
