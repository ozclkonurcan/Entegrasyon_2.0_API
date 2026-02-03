using Application.Interfaces.ConnectionModule;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Context;
using Persistence.Repositories.ConnectionModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories.EntegrasyonModulu.WTPartRepositories;
using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Interfaces.UsersModule;
using Persistence.Repositories.UsersModule;
using Application.Interfaces.AuthModule;
using Persistence.Repositories.AuthModule;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Application.Interfaces.ConnectionModule.WindchillConnectionModule;
using Persistence.Repositories.ConnectionModule.WindchillConnectionModule;
using Application.Interfaces.DatabaseManagementModule;
using Persistence.Repositories.DatabaseManagementModule;
using Application.Interfaces.WindchillModule;
using Persistence.Repositories.WindchillModule;
using Application.Interfaces.LogModule;
using Persistence.Repositories.LogModule;
using Persistence.Services;
using Application.Interfaces.IntegrationSettings;
using Persistence.Repositories.IntegrationSettings;
using Application.Common.Interfaces;
using Application.Interfaces.ApiService;
using Application.Interfaces.Notification;
using Infrastructure.Adapters.ApiServices;
using Infrastructure.Adapters.Services;
using Application.Interfaces.Generic;
using Persistence.Repositories.Generic;
using Application.Interfaces.EntegrasyonModulu.EMPDocumentServices;
using Persistence.Repositories.EntegrasyonModulu.EPMDocumentRepositories;

namespace Persistence;

public static class PersistenceServiceRegistration
{
	public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
	{
		//services.AddDbContext<BaseDbContexts>((serviceProvider, options) =>
		//{
		//	options.UseSqlServer(serviceProvider.GetRequiredService<IConfiguration>().GetConnectionString(configuration["SQL_CONNECTION_STRING_ADRESS"]))
		//		   .ReplaceService<IModelCacheKeyFactory, DynamicSchemaModelCacheKeyFactory>();
		//}, ServiceLifetime.Scoped);

		//services.AddDbContext<BaseDbContexts>((serviceProvider, options) =>
		//{
		//	var config = serviceProvider.GetRequiredService<IConfiguration>();
		//	options.UseSqlServer(config.GetConnectionString("DefaultConnection"))
		//		   .ReplaceService<IModelCacheKeyFactory, DynamicSchemaModelCacheKeyFactory>();
		//}, ServiceLifetime.Scoped);

		#region DbContext Settings
		//Yeni
		services.AddDbContext<BaseDbContexts>((serviceProvider, options) =>
		{
			var config = serviceProvider.GetRequiredService<IConfiguration>();
			options.UseSqlServer(
				config.GetConnectionString("DefaultConnection"),
				sqlServerOptions =>
				{
					sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);

					//Timeout ayarları
					sqlServerOptions.CommandTimeout(300); // 5 dakika

					// Deadlock ve geçici hata retry mekanizması bunu iptal ettik TransactionScope ile çakışıyor
					//sqlServerOptions.EnableRetryOnFailure(
					//	maxRetryCount: 5,
					//	maxRetryDelay: TimeSpan.FromSeconds(30),
					//	errorNumbersToAdd: new[] {
					//		1205, // Deadlock
					//               1222, // Lock timeout
					//               3902, // Transaction error
					//               2,    // Timeout
					//               -2    // Timeout (alternative)
					//	});
				})
				.ReplaceService<IModelCacheKeyFactory, DynamicSchemaModelCacheKeyFactory>();
		}, ServiceLifetime.Scoped);
		//Eski
		//services.AddDbContext<BaseDbContexts>((serviceProvider, options) =>
		//{
		//	var config = serviceProvider.GetRequiredService<IConfiguration>();
		//	options.UseSqlServer(
		//		config.GetConnectionString("DefaultConnection"),
		//		sqlServerOptions => sqlServerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
		//		   .ReplaceService<IModelCacheKeyFactory, DynamicSchemaModelCacheKeyFactory>();
		//}, ServiceLifetime.Scoped);
		#endregion


		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer(options =>
		{
			options.TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = configuration["Jwt:Issuer"],
				ValidAudience = configuration["Jwt:Audience"],
				IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Jwt:SecretKey"]))
			};
		});
		services.AddHttpClient();

		//Bunu özel yaptık eğer tutarsa sonraki yapacağıımız bütün işlemleri bunun üzerinden götürücez alttakilerinde bir kısmını kaldırıp bunu kullanıcaz kod tassarufu felaket artacak
		services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
		// Retry Repository
		services.AddScoped(typeof(IRetryRepository<>), typeof(RetryRepository<>));

		services.AddScoped<ILogService, LogRepository>();
		services.AddScoped<IWindchillService, WindchillRepository>();
		services.AddScoped<IDatabaseManagementService, DatabaseManagementRepository>();
		services.AddScoped<IAuthService, AuthRepository>();
		services.AddScoped<IUserService, UserRepository>();
		//services.AddScoped<IWTPartService, WTPartRepository>();
		services.AddScoped(typeof(IWTPartService<>), typeof(WTPartRepository<>));
		services.AddScoped<IStateService, StateRepository>();
		services.AddScoped<IEPMDocumentStateService, EPMDocumentStateRepository>();
		services.AddScoped<IConnectionService, ConnectionRepository>();
		services.AddScoped<IWindchillConnectionService, WindchillConnectionRepository>();
		services.AddScoped<IIntegrationSettingsService, IntegrationSettingsRepository>();

		//services.AddScoped<IEPMDocumentEquivalenceService, EPMDocumentEquivalenceManager>();

		services.AddScoped<IApiClientService, ApiClientService>();
		services.AddScoped<IEmailService, EmailService>();
		services.AddScoped<INotificationService, NotificationService>();
		services.AddScoped<INotificationDbContext>(provider =>
	provider.GetRequiredService<BaseDbContexts>());
		return services;
	}
}
