using Application.Common.Interfaces;
using Application.Interfaces.ApiService;
using Application.Interfaces.Generic;
using Application.Interfaces.Notification;
using Infrastructure.Adapters.ApiServices;
using Infrastructure.Adapters.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure;

public static class InfrastructureServiceRegistration
{
	public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
	{


		// HttpClient factory konfigürasyonu
		services.AddHttpClient("WindchillAPI", client =>
		{
			client.Timeout = TimeSpan.FromMinutes(5);
			client.DefaultRequestHeaders.Add("User-Agent", "DesignTech-Integration/1.0");
		})
		.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
		{
			MaxConnectionsPerServer = 10,
			UseCookies = false,
			UseProxy = false
		})
		.AddPolicyHandler(GetRetryPolicy())
		.AddPolicyHandler(GetCircuitBreakerPolicy());

		// Retry Policy
		static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
		{
			return HttpPolicyExtensions
				.HandleTransientHttpError()
				.OrResult(msg => !msg.IsSuccessStatusCode)
				.WaitAndRetryAsync(
					retryCount: 3,
					sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
					onRetry: (outcome, timespan, retryCount, context) =>
					{
						Console.WriteLine($"Retry {retryCount} after {timespan} seconds");
					});
		}

		// Circuit Breaker Policy
		static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
		{
			return HttpPolicyExtensions
				.HandleTransientHttpError()
				.CircuitBreakerAsync(
					handledEventsAllowedBeforeBreaking: 5,
					durationOfBreak: TimeSpan.FromSeconds(30));
		}

		// Retry Service
		services.AddScoped(typeof(IRetryService<>), typeof(RetryService<>));

		return services;
	}
}
