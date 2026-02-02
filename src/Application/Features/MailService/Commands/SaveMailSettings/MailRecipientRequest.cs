namespace Application.Features.MailService.Commands.SaveMailSettings;

public class MailRecipientRequest
{
	public string EmailAddress { get; set; }
	public string DisplayName { get; set; }
}