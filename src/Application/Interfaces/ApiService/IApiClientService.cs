using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.ApiService;

public interface IApiClientService
{
	Task<T> GetAsync<T>(string endpoint, Dictionary<string, string>? headers = null);
	//Task<T> GetAsync<T>(string endpoint);
	Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);
	Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest data);
	Task<bool> DeleteAsync(string endpoint);
}
