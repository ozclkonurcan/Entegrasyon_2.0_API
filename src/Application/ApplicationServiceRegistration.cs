using Application.Interfaces.Mail;
using Application.Pipelines.Logging;
using Application.Pipelines.MailNotification;
using Application.Pipelines.Notification;
using Application.Pipelines.RequireSystemCheck;
using Application.Pipelines.SqlConnectionCheck;
using Application.Pipelines.Transaction;
using Application.Pipelines.Validation;
using Application.Pipelines.WTPartLogging;
using Application.Pipelines.WTPartLogging.WTPartAlternateLogging;
using Application.Services.Mail;
using CrossCuttingConcerns.Serilog;
using CrossCuttingConcerns.Serilog.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application;

public static class ApplicationServiceRegistration
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{

		services.AddAutoMapper(Assembly.GetExecutingAssembly());
		services.AddSubClassesOfType(Assembly.GetExecutingAssembly(), typeof(BaseBusinessRules));


		services.AddMediatR(cfg =>
		{
			cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
			cfg.AddOpenBehavior(typeof(SystemCheckBehavior<,>));
			cfg.AddOpenBehavior(typeof(RequestValidationBehavior<,>));
			cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
			cfg.AddOpenBehavior(typeof(WTPartLoggingBehavior<,>));
			cfg.AddOpenBehavior(typeof(WTPartAlternateLoggingBehavior<,>));
			cfg.AddOpenBehavior(typeof(TransactionScopeBehavior<,>));
			cfg.AddOpenBehavior(typeof(SqlConnectionCheckBehavior<,>));
			cfg.AddOpenBehavior(typeof(ErrorNotificationBehavior<,>));
		});


		//services.AddSingleton<LoggerServiceBase, FileLogger>();
		services.AddSingleton<LoggerServiceBase, MsSqlLogger>();
		services.AddSingleton<WTPartMsSqlLogger>();
		services.AddSingleton<WTPartAlternateMsSqlLogger>();


		//Mail
		services.AddScoped<IMailService, MailService>();
		services.AddScoped<MailTemplateService>();




		services.AddLogging(builder =>
	builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Error));







		return services;
	}

	public static IServiceCollection AddSubClassesOfType(
		this IServiceCollection services,
		Assembly assembly,
		Type type,
		Func<IServiceCollection, Type, IServiceCollection>? addWithLifeCycle = null
		)
	{
		var types = assembly.GetTypes().Where(t => t.IsSubclassOf(type) && type != t).ToList();
		foreach (var item in types)
			if (addWithLifeCycle == null)
				services.AddScoped(item);
			else
				addWithLifeCycle(services, type);
		return services;
	}
}
