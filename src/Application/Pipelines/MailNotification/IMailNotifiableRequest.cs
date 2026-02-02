using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Pipelines.MailNotification;

public interface IMailNotifiableRequest
{
	// Mail gönderilecek durumları belirle
	bool SendOnSuccess { get; }
	bool SendOnError { get; }
	bool SendOnFinalFailure { get; }

	// Mail içeriği için gerekli bilgiler
	string GetMailSubject();
	string GetMailBody(bool isSuccess, string errorMessage = null);
	string GetEntityType(); // "WTPart", "Integration" vs.
	long? GetEntityId(); // Hangi kayıt için işlem yapıldı
}