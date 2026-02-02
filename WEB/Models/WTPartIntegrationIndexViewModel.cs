namespace WEB.Models;

public class WTPartIntegrationIndexViewModel
{
	// Gönderilmiş veriler için
	public int ReleasedSentCount { get; set; }
	public int CancelledSentCount { get; set; }

	// Gönderilmeyi bekleyen veriler için
	public int ReleasedNotSentCount { get; set; }
	public int CancelledNotSentCount { get; set; }

	// Hata bekleyen veriler için
	public int ReleasedErrorCount { get; set; }
	public int CancelledErrorCount { get; set; }
	public int WtpartAlternateErrorCount { get; set; }
	public int WtpartAlternateRemovedErrorCount { get; set; }

	//Muadil / Alternate
	public int WtpartAlternateCount { get; set; }
	public int WtpartAlternateSentCount { get; set; }


	//Muadil Removed / Alternate Removed
	public int WtpartAlternateRemovedCount { get; set; }
	public int WtpartAlternateRemovedSentCount { get; set; }
	// Endpoint detayları (örneğin, rol mapping’ten gelen endpoint listeleri – burada örnek placeholder veriler kullanıyoruz)
	public List<RoleMappingEndpointViewModel> ReleasedEndpoints { get; set; } = new List<RoleMappingEndpointViewModel>();
	public List<RoleMappingEndpointViewModel> CancelledEndpoints { get; set; } = new List<RoleMappingEndpointViewModel>();
}