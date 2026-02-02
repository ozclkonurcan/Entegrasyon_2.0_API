using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.MailService.Commands.TestMailConnection;
public class TestMailConnectionCommand : IRequest<TestMailConnectionResponse>
{
	public string SmtpServer { get; set; }
	public int SmtpPort { get; set; }
	public string SmtpUsername { get; set; }
	public string SmtpPassword { get; set; }
	public bool EnableSsl { get; set; }
	public string FromEmail { get; set; }
	public string TestEmail { get; set; } // Test maili gönderilecek adres

	public class TestMailConnectionHandler : IRequestHandler<TestMailConnectionCommand, TestMailConnectionResponse>
	{
		public async Task<TestMailConnectionResponse> Handle(TestMailConnectionCommand request, CancellationToken cancellationToken)
		{
			try
			{
				using var client = new SmtpClient(request.SmtpServer, request.SmtpPort)
				{
					Credentials = new NetworkCredential(request.SmtpUsername, request.SmtpPassword),
					EnableSsl = request.EnableSsl
				};

				var testMessage = new MailMessage
				{
					From = new MailAddress(request.FromEmail),
					Subject = "Mail Bağlantı Testi",
					Body = "Bu bir test mailidir. Mail ayarlarınız başarıyla çalışıyor!",
					IsBodyHtml = false
				};

				testMessage.To.Add(request.TestEmail);

				await client.SendMailAsync(testMessage);

				return new TestMailConnectionResponse
				{
					Success = true,
					Message = "Mail bağlantısı başarıyla test edildi!"
				};
			}
			catch (Exception ex)
			{
				return new TestMailConnectionResponse
				{
					Success = false,
					Message = $"Mail bağlantı testi başarısız: {ex.Message}"
				};
			}
		}
	}
}