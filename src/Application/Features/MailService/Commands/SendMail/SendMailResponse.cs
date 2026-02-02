using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.MailService.Commands.SendMail;

public class SendMailResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }
	public int SuccessCount { get; set; }
	public int ErrorCount { get; set; }
	public List<string> Errors { get; set; } = new List<string>();
}