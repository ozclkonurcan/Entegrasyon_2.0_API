namespace WEB.Interfaces;

public interface IGetTokenService
{
	Task<string> GetTokenAsync();
}
