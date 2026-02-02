using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Mail;

public class MailTemplateService
{
	public string GetErrorTemplate(string entityType, string entityNumber, string entityName, string errorMessage)
	{
		return entityType.ToUpper() switch
		{
			"WTPARTRELEASED" => GetWTPartReleasedErrorTemplate(entityNumber, entityName, errorMessage),
			"WTPARTCANCELLED" => GetWTPartCancelledErrorTemplate(entityNumber, entityName, errorMessage),
			"WTPARTALTERNATE" => GetWTPartAlternateErrorTemplate(entityNumber, entityName, errorMessage),
			_ => GetGenericErrorTemplate(entityType, entityNumber, entityName, errorMessage)
		};
	}

	public string GetSuccessTemplate(string entityType, string entityNumber, string entityName, string successMessage)
	{
		return entityType.ToUpper() switch
		{
			"WTPARTRELEASED" => GetWTPartReleasedSuccessTemplate(entityNumber, entityName, successMessage),
			"WTPARTCANCELLED" => GetWTPartCancelledSuccessTemplate(entityNumber, entityName, successMessage),
			"WTPARTALTERNATE" => GetWTPartAlternateSuccessTemplate(entityNumber, entityName, successMessage),
			_ => GetGenericSuccessTemplate(entityType, entityNumber, entityName, successMessage)
		};
	}

	public string GetSubject(string entityType, bool isSuccess, string entityNumber)
	{
		var status = isSuccess ? "✅ Başarılı" : "❌ Hata";
		var operation = entityType.ToUpper() switch
		{
			"WTPARTRELEASED" => "WTPart Released",
			"WTPARTCANCELLED" => "WTPart Cancelled",
			"WTPARTALTERNATE" => "WTPart Alternate",
			_ => entityType
		};

		return $"{status} - {operation} - {entityNumber}";
	}

	#region Private Templates

	private string GetWTPartReleasedErrorTemplate(string number, string name, string error)
	{
		var isCustomerIssue = IsCustomerResponsibility(error);
		var actionMessage = GetActionMessage(error);

		var bgColor = isCustomerIssue ? "#fff3cd" : "#e8f5e8";
		var borderColor = isCustomerIssue ? "#ffeaa7" : "#4caf50";
		var textColor = isCustomerIssue ? "#856404" : "#2e7d32";

		// Bizim sorunumuzsa destek bilgilerini ekle
		var supportInfo = !isCustomerIssue ? @"
            <div style='margin-top: 15px; padding: 10px; background-color: #e3f2fd; border: 1px solid #90caf9; border-radius: 5px;'>
                <h4 style='margin: 0 0 5px 0; color: #1565c0;'>📞 Destek İletişim:</h4>
                <p style='margin: 0; color: #1565c0; font-size: 14px;'>
                    <strong>Email:</strong> destek@firmaniz.com<br>
                    <strong>Tel:</strong> +90 XXX XXX XX XX<br>
                    <strong>Çalışma Saatleri:</strong> 09:00 - 18:00
                </p>
            </div>" : "";

		return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #dc3545; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
                    <h2 style='margin: 0;'>❌ WTPart Released İşlemi Başarısız</h2>
                </div>
                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Parça Numarası:</td><td style='padding: 8px;'>{number}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Parça Adı:</td><td style='padding: 8px;'>{name}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>Released</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Hata Mesajı:</td><td style='padding: 8px; color: #dc3545;'>{error}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
                    </table>
                    
                    <div style='margin-top: 20px; padding: 15px; background-color: {bgColor}; border: 1px solid {borderColor}; border-radius: 5px;'>
                        <p style='margin: 0; color: {textColor}; font-weight: bold;'>
                            {actionMessage}
                        </p>
                    </div>
                    
                    {supportInfo}
                </div>
            </div>
        ";
	}

	private string GetWTPartCancelledErrorTemplate(string number, string name, string error)
	{
		var isCustomerIssue = IsCustomerResponsibility(error);
		var actionMessage = GetActionMessage(error);

		var bgColor = isCustomerIssue ? "#fff3cd" : "#e8f5e8";
		var borderColor = isCustomerIssue ? "#ffeaa7" : "#4caf50";
		var textColor = isCustomerIssue ? "#856404" : "#2e7d32";

		var supportInfo = !isCustomerIssue ? @"
            <div style='margin-top: 15px; padding: 10px; background-color: #e3f2fd; border: 1px solid #90caf9; border-radius: 5px;'>
                <h4 style='margin: 0 0 5px 0; color: #1565c0;'>📞 Destek İletişim:</h4>
                <p style='margin: 0; color: #1565c0; font-size: 14px;'>
                    <strong>Email:</strong> destek@firmaniz.com<br>
                    <strong>Tel:</strong> +90 XXX XXX XX XX<br>
                    <strong>Çalışma Saatleri:</strong> 09:00 - 18:00
                </p>
            </div>" : "";

		return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #dc3545; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
                    <h2 style='margin: 0;'>❌ WTPart Cancelled İşlemi Başarısız</h2>
                </div>
                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Parça Numarası:</td><td style='padding: 8px;'>{number}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Parça Adı:</td><td style='padding: 8px;'>{name}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>Cancelled</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Hata Mesajı:</td><td style='padding: 8px; color: #dc3545;'>{error}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
                    </table>
                    
                    <div style='margin-top: 20px; padding: 15px; background-color: {bgColor}; border: 1px solid {borderColor}; border-radius: 5px;'>
                        <p style='margin: 0; color: {textColor}; font-weight: bold;'>
                            {actionMessage}
                        </p>
                    </div>
                    
                    {supportInfo}
                </div>
            </div>
        ";
	}

	private string GetWTPartAlternateErrorTemplate(string number, string name, string error)
	{
		var isCustomerIssue = IsCustomerResponsibility(error);
		var actionMessage = GetActionMessage(error);

		var bgColor = isCustomerIssue ? "#fff3cd" : "#e8f5e8";
		var borderColor = isCustomerIssue ? "#ffeaa7" : "#4caf50";
		var textColor = isCustomerIssue ? "#856404" : "#2e7d32";

		var supportInfo = !isCustomerIssue ? @"
            <div style='margin-top: 15px; padding: 10px; background-color: #e3f2fd; border: 1px solid #90caf9; border-radius: 5px;'>
                <h4 style='margin: 0 0 5px 0; color: #1565c0;'>📞 Destek İletişim:</h4>
                <p style='margin: 0; color: #1565c0; font-size: 14px;'>
                    <strong>Email:</strong> destek@firmaniz.com<br>
                    <strong>Tel:</strong> +90 XXX XXX XX XX<br>
                    <strong>Çalışma Saatleri:</strong> 09:00 - 18:00
                </p>
            </div>" : "";

		return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #dc3545; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
                    <h2 style='margin: 0;'>❌ WTPart Alternate İşlemi Başarısız</h2>
                </div>
                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Parça Numarası:</td><td style='padding: 8px;'>{number}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Parça Adı:</td><td style='padding: 8px;'>{name}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>Alternate Link</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Hata Mesajı:</td><td style='padding: 8px; color: #dc3545;'>{error}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
                    </table>
                    
                    <div style='margin-top: 20px; padding: 15px; background-color: {bgColor}; border: 1px solid {borderColor}; border-radius: 5px;'>
                        <p style='margin: 0; color: {textColor}; font-weight: bold;'>
                            {actionMessage}
                        </p>
                    </div>
                    
                    {supportInfo}
                </div>
            </div>
        ";
	}

	private string GetGenericErrorTemplate(string entityType, string number, string name, string error)
	{
		var isCustomerIssue = IsCustomerResponsibility(error);
		var actionMessage = GetActionMessage(error);

		var bgColor = isCustomerIssue ? "#fff3cd" : "#e8f5e8";
		var borderColor = isCustomerIssue ? "#ffeaa7" : "#4caf50";
		var textColor = isCustomerIssue ? "#856404" : "#2e7d32";

		var supportInfo = !isCustomerIssue ? @"
            <div style='margin-top: 15px; padding: 10px; background-color: #e3f2fd; border: 1px solid #90caf9; border-radius: 5px;'>
                <h4 style='margin: 0 0 5px 0; color: #1565c0;'>📞 Destek İletişim:</h4>
                <p style='margin: 0; color: #1565c0; font-size: 14px;'>
                    <strong>Email:</strong> destek@firmaniz.com<br>
                    <strong>Tel:</strong> +90 XXX XXX XX XX<br>
                    <strong>Çalışma Saatleri:</strong> 09:00 - 18:00
                </p>
            </div>" : "";

		return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #dc3545; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
                    <h2 style='margin: 0;'>❌ {entityType} İşlemi Başarısız</h2>
                </div>
                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Entity:</td><td style='padding: 8px;'>{number} - {name}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>{entityType}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Hata Mesajı:</td><td style='padding: 8px; color: #dc3545;'>{error}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
                    </table>
                    
                    <div style='margin-top: 20px; padding: 15px; background-color: {bgColor}; border: 1px solid {borderColor}; border-radius: 5px;'>
                        <p style='margin: 0; color: {textColor}; font-weight: bold;'>
                            {actionMessage}
                        </p>
                    </div>
                    
                    {supportInfo}
                </div>
            </div>
        ";
	}

	// Success template'ler aynı kalıyor - onlarda zaten sorun yok
	private string GetWTPartReleasedSuccessTemplate(string number, string name, string message)
	{
		return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #28a745; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
                    <h2 style='margin: 0;'>✅ WTPart Released İşlemi Başarılı</h2>
                </div>
                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Parça Numarası:</td><td style='padding: 8px;'>{number}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Parça Adı:</td><td style='padding: 8px;'>{name}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>Released</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Mesaj:</td><td style='padding: 8px; color: #28a745;'>{message}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
                    </table>
                </div>
            </div>
        ";
	}

	private string GetWTPartCancelledSuccessTemplate(string number, string name, string message)
	{
		return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #28a745; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
                    <h2 style='margin: 0;'>✅ WTPart Cancelled İşlemi Başarılı</h2>
                </div>
                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Parça Numarası:</td><td style='padding: 8px;'>{number}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Parça Adı:</td><td style='padding: 8px;'>{name}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>Cancelled</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Mesaj:</td><td style='padding: 8px; color: #28a745;'>{message}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
                    </table>
                </div>
            </div>
        ";
	}

	private string GetWTPartAlternateSuccessTemplate(string number, string name, string message)
	{
		return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #28a745; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
                    <h2 style='margin: 0;'>✅ WTPart Alternate İşlemi Başarılı</h2>
                </div>
                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Parça Numarası:</td><td style='padding: 8px;'>{number}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Parça Adı:</td><td style='padding: 8px;'>{name}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>Alternate Link</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Mesaj:</td><td style='padding: 8px; color: #28a745;'>{message}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
                    </table>
                </div>
            </div>
        ";
	}

	private string GetGenericSuccessTemplate(string entityType, string number, string name, string message)
	{
		return $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background-color: #28a745; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
                    <h2 style='margin: 0;'>✅ {entityType} İşlemi Başarılı</h2>
                </div>
                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
                    <table style='width: 100%; border-collapse: collapse;'>
                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Entity:</td><td style='padding: 8px;'>{number} - {name}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>{entityType}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Mesaj:</td><td style='padding: 8px; color: #28a745;'>{message}</td></tr>
                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
                    </table>
                </div>
            </div>
        ";
	}

	#region Helper Methods

	private string GetActionMessage(string errorMessage)
	{
		var error = errorMessage.ToLower();

		// API Bağlantı Hataları (Müşteri Sorunu)
		if (error.Contains("bağlantı kurulamadı") || error.Contains("connection refused") ||
			error.Contains("connection failed") || error.Contains("no connection could be made"))
		{
			return "🔧 Lütfen hedef API'nizin aktif olup olmadığını ve bağlantı ayarlarınızı kontrol ediniz.";
		}

		// Timeout Hataları (Müşteri Sorunu)
		if (error.Contains("timeout") || error.Contains("zaman aşımı"))
		{
			return "⏱️ API yanıt süresi çok uzun. Lütfen hedef API'nizin performansını kontrol ediniz.";
		}

		// 404 Not Found (Müşteri Sorunu)
		if (error.Contains("404") || error.Contains("not found"))
		{
			return "🔍 API endpoint'i bulunamadı. Lütfen API URL'inizi kontrol ediniz.";
		}

		// 401 Unauthorized (Müşteri Sorunu)
		if (error.Contains("401") || error.Contains("unauthorized"))
		{
			return "🔐 Yetkilendirme hatası. Lütfen API kullanıcı adı ve şifrenizi kontrol ediniz.";
		}

		// 403 Forbidden (Müşteri Sorunu)
		if (error.Contains("403") || error.Contains("forbidden"))
		{
			return "🚫 Erişim reddedildi. Lütfen API yetkilendirme ayarlarınızı kontrol ediniz.";
		}

		// 500 Internal Server Error (Müşteri Sorunu)
		if (error.Contains("500") || error.Contains("internal server error"))
		{
			return "⚠️ Hedef API'de sunucu hatası oluştu. Lütfen API'nizin durumunu kontrol ediniz.";
		}

		// 502 Bad Gateway (Müşteri Sorunu)
		if (error.Contains("502") || error.Contains("bad gateway"))
		{
			return "🌐 Gateway hatası. Lütfen ağ bağlantınızı ve proxy ayarlarınızı kontrol ediniz.";
		}

		// 503 Service Unavailable (Müşteri Sorunu)
		if (error.Contains("503") || error.Contains("service unavailable"))
		{
			return "🔧 Hedef API şu anda hizmet vermiyor. Lütfen API'nizin aktif olup olmadığını kontrol ediniz.";
		}

		// SSL/TLS Hataları (Müşteri Sorunu)
		if (error.Contains("ssl") || error.Contains("tls") || error.Contains("certificate"))
		{
			return "🔒 SSL sertifika hatası. Lütfen HTTPS ayarlarınızı ve sertifikalarınızı kontrol ediniz.";
		}

		// JSON Parse Hataları (Bizim Sorunumuz)
		if (error.Contains("json") || error.Contains("parse") || error.Contains("deserialize"))
		{
			return "🔄 Sistem otomatik olarak tekrar deneyecektir. Sorun devam ederse lütfen destek ekibi ile iletişime geçiniz.";
		}

		// Database Hataları (Bizim Sorunumuz)
		if (error.Contains("database") || error.Contains("sql") || error.Contains("connection string"))
		{
			return "🔄 Sistem otomatik olarak tekrar deneyecektir. Sorun devam ederse lütfen destek ekibi ile iletişime geçiniz.";
		}

		// Null Reference (Bizim Sorunumuz)
		if (error.Contains("nullreference") || error.Contains("object reference"))
		{
			return "🔄 Sistem otomatik olarak tekrar deneyecektir. Sorun devam ederse lütfen destek ekibi ile iletişime geçiniz.";
		}

		// Windchill API Hataları (Bizim Sorunumuz)
		if (error.Contains("windchill") || error.Contains("wt"))
		{
			return "🔄 Sistem otomatik olarak tekrar deneyecektir. Sorun devam ederse lütfen destek ekibi ile iletişime geçiniz.";
		}

		// Genel müşteri sorunu (bilinmeyen API hatası)
		if (error.Contains("api") || error.Contains("http"))
		{
			return "🔧 Lütfen API ayarlarınızı ve hedef sistemin durumunu kontrol ediniz.";
		}

		// Diğer tüm hatalar (bizim sorunumuz)
		return "🔄 Sistem otomatik olarak tekrar deneyecektir. Sorun devam ederse lütfen destek ekibi ile iletişime geçiniz.";
	}

	private bool IsCustomerResponsibility(string errorMessage)
	{
		var error = errorMessage.ToLower();

		// Müşteri sorumluluğundaki hatalar
		var customerErrors = new[]
		{
			"bağlantı kurulamadı",
			"connection refused",
			"connection failed",
			"no connection could be made",
			"timeout",
			"zaman aşımı",
			"404",
			"not found",
			"401",
			"unauthorized",
			"403",
			"forbidden",
			"500",
			"internal server error",
			"502",
			"bad gateway",
			"503",
			"service unavailable",
			"ssl",
			"tls",
			"certificate"
		};

		return customerErrors.Any(err => error.Contains(err));
	}

	#endregion

	#endregion
}













//Eski
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Application.Services.Mail;


//public class MailTemplateService
//{
//	public string GetErrorTemplate(string entityType, string entityNumber, string entityName, string errorMessage)
//	{
//		return entityType.ToUpper() switch
//		{
//			"WTPARTRELEASED" => GetWTPartReleasedErrorTemplate(entityNumber, entityName, errorMessage),
//			"WTPARTCANCELLED" => GetWTPartCancelledErrorTemplate(entityNumber, entityName, errorMessage),
//			"WTPARTALTERNATE" => GetWTPartAlternateErrorTemplate(entityNumber, entityName, errorMessage),
//			_ => GetGenericErrorTemplate(entityType, entityNumber, entityName, errorMessage)
//		};
//	}

//	public string GetSuccessTemplate(string entityType, string entityNumber, string entityName, string successMessage)
//	{
//		return entityType.ToUpper() switch
//		{
//			"WTPARTRELEASED" => GetWTPartReleasedSuccessTemplate(entityNumber, entityName, successMessage),
//			"WTPARTCANCELLED" => GetWTPartCancelledSuccessTemplate(entityNumber, entityName, successMessage),
//			"WTPARTALTERNATE" => GetWTPartAlternateSuccessTemplate(entityNumber, entityName, successMessage),
//			_ => GetGenericSuccessTemplate(entityType, entityNumber, entityName, successMessage)
//		};
//	}

//	public string GetSubject(string entityType, bool isSuccess, string entityNumber)
//	{
//		var status = isSuccess ? "✅ Başarılı" : "❌ Hata";
//		var operation = entityType.ToUpper() switch
//		{
//			"WTPARTRELEASED" => "WTPart Released",
//			"WTPARTCANCELLED" => "WTPart Cancelled",
//			"WTPARTALTERNATE" => "WTPart Alternate",
//			_ => entityType
//		};

//		return $"{status} - {operation} - {entityNumber}";
//	}

//	#region Private Templates

//	private string GetWTPartReleasedErrorTemplate(string number, string name, string error)
//	{
//		return $@"
//            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
//                <div style='background-color: #dc3545; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
//                    <h2 style='margin: 0;'>❌ WTPart Released İşlemi Başarısız</h2>
//                </div>
//                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
//                    <table style='width: 100%; border-collapse: collapse;'>
//                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Parça Numarası:</td><td style='padding: 8px;'>{number}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Parça Adı:</td><td style='padding: 8px;'>{name}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>Released</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Hata Mesajı:</td><td style='padding: 8px; color: #dc3545;'>{error}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
//                    </table>

//                    <div style='margin-top: 20px; padding: 15px; background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px;'>
//                        <h4 style='margin: 0 0 10px 0; color: #856404;'>🔍 Olası Nedenler:</h4>
//                        <ul style='margin: 0; color: #856404;'>
//                            <li>Windchill API bağlantı sorunu</li>
//                            <li>Hedef sistem erişim hatası</li>
//                            <li>Parça Windchill'de bulunamadı</li>
//                            <li>JSON parse hatası</li>
//                        </ul>
//                    </div>
//                </div>
//            </div>
//        ";
//	}

//	private string GetWTPartCancelledErrorTemplate(string number, string name, string error)
//	{
//		return $@"
//            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
//                <div style='background-color: #dc3545; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
//                    <h2 style='margin: 0;'>❌ WTPart Cancelled İşlemi Başarısız</h2>
//                </div>
//                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
//                    <table style='width: 100%; border-collapse: collapse;'>
//                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Parça Numarası:</td><td style='padding: 8px;'>{number}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Parça Adı:</td><td style='padding: 8px;'>{name}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>Cancelled</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Hata Mesajı:</td><td style='padding: 8px; color: #dc3545;'>{error}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
//                    </table>
//                </div>
//            </div>
//        ";
//	}

//	private string GetWTPartAlternateErrorTemplate(string number, string name, string error)
//	{
//		return $@"
//            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
//                <div style='background-color: #dc3545; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
//                    <h2 style='margin: 0;'>❌ WTPart Alternate İşlemi Başarısız</h2>
//                </div>
//                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
//                    <table style='width: 100%; border-collapse: collapse;'>
//                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Parça Numarası:</td><td style='padding: 8px;'>{number}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Parça Adı:</td><td style='padding: 8px;'>{name}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>Alternate Link</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Hata Mesajı:</td><td style='padding: 8px; color: #dc3545;'>{error}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
//                    </table>
//                </div>
//            </div>
//        ";
//	}

//	private string GetGenericErrorTemplate(string entityType, string number, string name, string error)
//	{
//		return $@"
//            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
//                <div style='background-color: #dc3545; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
//                    <h2 style='margin: 0;'>❌ {entityType} İşlemi Başarısız</h2>
//                </div>
//                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
//                    <table style='width: 100%; border-collapse: collapse;'>
//                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Entity:</td><td style='padding: 8px;'>{number} - {name}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>{entityType}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Hata Mesajı:</td><td style='padding: 8px; color: #dc3545;'>{error}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
//                    </table>
//                </div>
//            </div>
//        ";
//	}

//	private string GetWTPartReleasedSuccessTemplate(string number, string name, string message)
//	{
//		return $@"
//            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
//                <div style='background-color: #28a745; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
//                    <h2 style='margin: 0;'>✅ WTPart Released İşlemi Başarılı</h2>
//                </div>
//                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
//                    <table style='width: 100%; border-collapse: collapse;'>
//                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Parça Numarası:</td><td style='padding: 8px;'>{number}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Parça Adı:</td><td style='padding: 8px;'>{name}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>Released</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Mesaj:</td><td style='padding: 8px; color: #28a745;'>{message}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
//                    </table>
//                </div>
//            </div>
//        ";
//	}

//	private string GetWTPartCancelledSuccessTemplate(string number, string name, string message)
//	{
//		return $@"
//            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
//                <div style='background-color: #28a745; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
//                    <h2 style='margin: 0;'>✅ WTPart Cancelled İşlemi Başarılı</h2>
//                </div>
//                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
//                    <table style='width: 100%; border-collapse: collapse;'>
//                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Parça Numarası:</td><td style='padding: 8px;'>{number}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Parça Adı:</td><td style='padding: 8px;'>{name}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>Cancelled</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Mesaj:</td><td style='padding: 8px; color: #28a745;'>{message}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
//                    </table>
//                </div>
//            </div>
//        ";
//	}

//	private string GetWTPartAlternateSuccessTemplate(string number, string name, string message)
//	{
//		return $@"
//            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
//                <div style='background-color: #28a745; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
//                    <h2 style='margin: 0;'>✅ WTPart Alternate İşlemi Başarılı</h2>
//                </div>
//                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
//                    <table style='width: 100%; border-collapse: collapse;'>
//                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Parça Numarası:</td><td style='padding: 8px;'>{number}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Parça Adı:</td><td style='padding: 8px;'>{name}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>Alternate Link</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Mesaj:</td><td style='padding: 8px; color: #28a745;'>{message}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
//                    </table>
//                </div>
//            </div>
//        ";
//	}

//	private string GetGenericSuccessTemplate(string entityType, string number, string name, string message)
//	{
//		return $@"
//            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
//                <div style='background-color: #28a745; color: white; padding: 15px; border-radius: 5px 5px 0 0;'>
//                    <h2 style='margin: 0;'>✅ {entityType} İşlemi Başarılı</h2>
//                </div>
//                <div style='background-color: #f8f9fa; padding: 20px; border: 1px solid #dee2e6; border-radius: 0 0 5px 5px;'>
//                    <table style='width: 100%; border-collapse: collapse;'>
//                        <tr><td style='padding: 8px; font-weight: bold; width: 150px;'>Entity:</td><td style='padding: 8px;'>{number} - {name}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>İşlem Tipi:</td><td style='padding: 8px;'>{entityType}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Mesaj:</td><td style='padding: 8px; color: #28a745;'>{message}</td></tr>
//                        <tr><td style='padding: 8px; font-weight: bold;'>Zaman:</td><td style='padding: 8px;'>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</td></tr>
//                    </table>
//                </div>
//            </div>
//        ";
//	}

//	#endregion
//}