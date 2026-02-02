using System.DirectoryServices.Protocols;
using System.Net;

namespace WEB.Services;

public class LdapAuthenticationService
{
	private readonly string _ldapServer;
	private readonly int _ldapPort;
	private readonly string _baseDn;
	private readonly string _bindDn;
	private readonly string _bindPassword;

	public LdapAuthenticationService(string ldapServer, int ldapPort, string baseDn, string bindDn, string bindPassword)
	{
		_ldapServer = ldapServer;
		_ldapPort = ldapPort;
		_baseDn = baseDn;
		_bindDn = bindDn;
		_bindPassword = bindPassword;
	}

	public bool Authenticate(string username, string password)
	{
		try
		{
			// LDAP bağlantısı oluştur
			using (var ldapConnection = new LdapConnection(new LdapDirectoryIdentifier(_ldapServer, _ldapPort)))
			{
				// Debug bilgileri
				Console.WriteLine($"LDAP Server: {_ldapServer}:{_ldapPort}");
				Console.WriteLine($"Bind DN: {_bindDn}");

				// Yönetici kimlik bilgileri ile bağlan
				ldapConnection.AuthType = AuthType.Basic;
				ldapConnection.Bind(new NetworkCredential(_bindDn, _bindPassword));

				// Kullanıcı DN'si oluştur
				var userDn = $"uid={username},{_baseDn}";

				// Kullanıcı kimlik doğrulaması yap
				ldapConnection.Bind(new NetworkCredential(userDn, password));

				// Bağlantı başarılı ise kullanıcı doğrulandı
				return true;
			}
		}
		catch (LdapException ex)
		{
			// Hata detaylarını logla
			Console.WriteLine($"LDAP Hatası: {ex.Message}");
			Console.WriteLine($"Hata Kodu: {ex.ErrorCode}");
			Console.WriteLine($"Hata Detayı: {ex.ServerErrorMessage}");
			return false;
		}
		catch (Exception ex)
		{
			// Diğer hataları logla
			Console.WriteLine($"Genel Hata: {ex.Message}");
			return false;
		}
	}
}