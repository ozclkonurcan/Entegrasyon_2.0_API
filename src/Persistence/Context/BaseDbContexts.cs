using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Entities.Auth;
using Domain.Entities.EPMModels;
using Domain.Entities.IntegrationSettings;
using Domain.Entities.LogSettings;
using Domain.Entities.MailService;
using Domain.Entities.Notification;
using Domain.Entities.Settings;
using Domain.Entities.WTPartModels.AlternateModels;
using Domain.Entities.WTPartModels.AlternateRemovedModels;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Persistence.Configurations.WTPartEntityConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Context;



public class DynamicSchemaModelCacheKeyFactory : IModelCacheKeyFactory
{
	public object Create(DbContext context, bool designTime)
	{
		if (context is BaseDbContexts dbContext)
		{
			return (context.GetType(), dbContext.schemaName);
		}
		return context.GetType();
	}
}

public class BaseDbContexts : DbContext, INotificationDbContext
{
	public string schemaName { get; private set; }
	protected IConfiguration Configuration { get; set; }

	#region WTPart

	public DbSet<WTPart> WTParts { get; set; }
	public DbSet<WTPartError> WTPartErrors { get; set; }
	public DbSet<WTPartSentDatas> WTPartSentDatas { get; set; }
	public DbSet<WTPartAllLogs> WTPartAllLogs { get; set; }
	#endregion
	#region WTPART Alternate
	public DbSet<WTPartAlternateLinkEntegration> WTPartAlternateLinks { get; set; }
	public DbSet<WTPartAlternateLinkErrorEntegration> WTPartAlternateLinkErrors { get; set; }
	public DbSet<WTPartAlternateLinkSentEntegration> WTPartAlternateLinkSents { get; set; }
	public DbSet<WTPartAlternateLinkLogEntegration> WTPartAlternateLinkLogs { get; set; }
	public DbSet<WTPartAlternateLinkRemovedEntegration> WTPartAlternateLinkRemoveds { get; set; }
	public DbSet<WTPartAlternateLinkRemovedErrorEntegration> WTPartAlternateLinkRemovedErrors { get; set; }
	public DbSet<WTPartAlternateLinkRemovedSentEntegration> WTPartAlternateLinkRemovedSents { get; set; }
	public DbSet<WTPartAlternateLinkRemovedLogEntegration> WTPartAlternateLinkRemovedLogs { get; set; }
	#endregion

	#region EPMDocument
	public DbSet<EPMDocument> EPMDocument { get; set; }
	public DbSet<EPMDocument_RELEASED> EPMDocument_RELEASED { get; set; }
	public DbSet<EPMDocument_CANCELLED> EPMDocument_CANCELLED { get; set; }

	// YENİ EKLENENLER:
	public DbSet<EPMDocument_ERROR> EPMDocumentErrors { get; set; }
	public DbSet<EPMDocument_SENT> EPMDocumentSents { get; set; }


	public DbSet<EPMDocument_CANCELLED_SENT> EPMDocument_CANCELLED_SENT { get; set; }
	public DbSet<EPMDocument_CANCELLED_ERROR> EPMDocument_CANCELLED_ERROR { get; set; }

	public DbSet<EPMDocumentMaster> EPMDocumentMasters { get; set; }
	public DbSet<EPMReferenceLink> EPMReferenceLinks { get; set; }

	#endregion

	#region MailSettings
	public DbSet<MailSettings> MailSettings { get; set; }
	public DbSet<MailRecipient> MailRecipients { get; set; }
	#endregion

	public DbSet<User> Users { get; set; }
	public DbSet<LogEntry> Logs { get; set; }
	public DbSet<RoleMapping> RoleMappings { get; set; }
	public DbSet<IntegrationModuleSettings> IntegrationModuleSettings { get; set; }

	public DbSet<EmailSettings> EmailSettings { get; set; }
	public DbSet<EmailRecipient> EmailRecipients { get; set; }
	public DbSet<ErrorNotification> ErrorNotifications { get; set; }
	public DbSet<NotificationHistory> NotificationHistory { get; set; }

	public BaseDbContexts(DbContextOptions<BaseDbContexts> dbContextOptions, IConfiguration configuration)
		: base(dbContextOptions)
	{
		Env.Load();
		Configuration = configuration;
		try {
        var connection = Database.GetDbConnection();
        //Console.WriteLine($"Database connection: {connection.ConnectionString}");
        //Console.WriteLine($"Database: {connection.Database}");
        
        Database.EnsureCreated();
        //Console.WriteLine("Database connection successful");
    }
    catch (Exception ex) {
        Console.WriteLine($"Database connection error: {ex.Message}");
    }
		//Database.EnsureCreated();
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		Env.Load();
		string connectionString = Env.GetString("SQL_CONNECTION_STRING_ADRESS");
		//optionsBuilder.UseSqlServer(connectionString);
		optionsBuilder.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
		schemaName = Env.GetString("SQL_SCHEMA");
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(schemaName);
		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
	}
}

//public class BaseDbContexts : DbContext
//{
//	public string schemaName { get; private set; }
//	protected IConfiguration Configuration { get; set; }

//	public DbSet<WTPart> WTParts { get; set; }
//	public DbSet<User> Users { get; set; }
//	public DbSet<LogEntry> Logs { get; set; }


//	public BaseDbContexts(DbContextOptions dbContextOptions, IConfiguration configuration) : base(dbContextOptions)
//	{
//		Env.Load();
//		Configuration = configuration;
//		Database.EnsureCreated();
//	}

//	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//	{
//		Env.Load();
//		string connectionString = Env.GetString("SQL_CONNECTION_STRING_ADRESS");

//		optionsBuilder.UseSqlServer(connectionString);
//		schemaName = Env.GetString("SQL_SCHEMA");
//	}

//	protected override void OnModelCreating(ModelBuilder modelBuilder)
//	{
//		modelBuilder.HasDefaultSchema(schemaName);
//		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
//	}
//}




//public class BaseDbContexts : DbContext
//{
//	private readonly IConfiguration _configuration;
//	private readonly string _connectionString;
//	private readonly string _schemaName;

//	public string SchemaName => _schemaName; // SchemaName dışarıya açılabilir

//	public DbSet<WTPart> WTParts { get; set; }
//	public DbSet<User> Users { get; set; }
//	public DbSet<LogEntry> Logs { get; set; }

//	// Dinamik olarak veritabanı bağlantısı ve şema için constructor kullanıyoruz.
//	public BaseDbContexts(DbContextOptions<BaseDbContexts> dbContextOptions, IConfiguration configuration) : base(dbContextOptions)
//	{
//		_configuration = configuration;
//		_connectionString = _configuration.GetConnectionString("SQL_CONNECTION_STRING");  // Dinamik bağlantı
//		_schemaName = _configuration["SQL_SCHEMA"];  // Dinamik şema
//	}

//	// OnConfiguring metodunda her seferinde yeni bir bağlantı ve şema alıyoruz
//	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//	{
//		if (!optionsBuilder.IsConfigured)
//		{
//			optionsBuilder.UseSqlServer(_connectionString); // Dinamik bağlantı
//		}
//	}

//	// Model yapılandırması
//	protected override void OnModelCreating(ModelBuilder modelBuilder)
//	{
//		modelBuilder.HasDefaultSchema(_schemaName);  // Dinamik şema
//		modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
//	}
//}
