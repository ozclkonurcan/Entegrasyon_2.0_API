using WEB.Controllers;
using WEB.Models;
//using static WEB.Controllers.LoginSettings;

namespace WEB.Interfaces;

public interface IApiService
{
	Task<T> GetAsync<T>( string endpoint);

	Task<string> PostAsync<TRequest>(string endpoint, TRequest data);
	Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest data);
	Task<bool> DeleteAsync( string endpoint);

	Task<LoggedResponse> LoginAsync(LoginSettings loginSettings);
	Task LogoutAsync();
	Task<bool> ValidateTokenAsync( string token); // Token doğrulama metodu

	Task<bool> CheckTablesAsync();
	Task<bool> CheckSqlConnectionAsync(); 
	Task<bool> CheckSqlConnection(SqlConnectionModel model);
	Task<bool> CheckApiConnectionAsync(ApiConnectionModel model); 
	Task<bool> CheckWindchillConnection(WindchillConnectionSettings model); 
	Task<bool> CheckWindchillConnectionAsync(); 
	Task<bool> SetupTable(); 

	Task<List<WTUsers>> SearchUsersAsync(string searchTerm);

	Task<LoggedResponse> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest);
}

