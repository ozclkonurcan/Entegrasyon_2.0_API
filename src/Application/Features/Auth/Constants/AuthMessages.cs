using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Constants;

public static class AuthMessages
{
	public const string SectionName = "Auth";

	public const string EmailAuthenticatorDontExists = "EmailAuthenticatorBulunmamaktadır";
	public const string OtpAuthenticatorDontExists = "OtpAuthenticatorBulunmamaktadır";
	public const string AlreadyVerifiedOtpAuthenticatorIsExists = "ZatenDoğrulanmışOtpAuthenticatorMevcut";
	public const string EmailActivationKeyDontExists = "EmailAktivasyonAnahtarıBulunmamaktadır";
	public const string UserDontExists = "KullanıcıBulunmamaktadır";
	public const string UserHaveAlreadyAAuthenticator = "KullanıcınınZatenBirAuthenticatoruVar";
	public const string RefreshDontExists = "YenilemeBulunmamaktadır";
	public const string InvalidRefreshToken = "GeçersizYenilemeTokeni";
	public const string UserMailAlreadyExists = "KullanıcıMailiZatenMevcut";
	public const string PasswordDontMatch = "ŞifreEşleşmiyor";
}
