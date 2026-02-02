using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrossCuttingConcerns.Serilog.ConfigurationModels;

namespace CrossCuttingConcerns.Serilog.Logger;

public class WTPartMsSqlLogger : LoggerServiceBase
{
	public WTPartMsSqlLogger(IConfiguration configuration)
	{
		try
		{
			// .env dosyasını yükleyin (uygulamanızın content root dizininde bulunduğundan emin olun)
			Env.Load();

			// Bağlantı dizesi ve şema bilgisi ortam değişkenlerinden alınıyor.
			string connectionString = Env.GetString("SQL_CONNECTION_STRING_ADRESS");
			if (string.IsNullOrEmpty(connectionString))
				throw new Exception("Connection string bilgisi eksik.");

			string schemaName = Env.GetString("SQL_SCHEMA");
			if (string.IsNullOrEmpty(schemaName))
				throw new Exception("SQL schema bilgisi eksik.");

			// Tablo adı sabit olarak tanımlanıyor.
			string tableNameLog = "Des2_WTPart_Log";
			//string tableNameSent = "Des2_WTPart_Sent";

			// Sink ayarları: Her iki tablo da önceden oluşturulmuş olduğundan AutoCreateSqlTable false.
			MSSqlServerSinkOptions sinkOptionsLog = new MSSqlServerSinkOptions
			{
				TableName = tableNameLog,
				AutoCreateSqlTable = false,
				SchemaName = schemaName
			};

			//MSSqlServerSinkOptions sinkOptionsSent = new MSSqlServerSinkOptions
			//{
			//	TableName = tableNameSent,
			//	AutoCreateSqlTable = false,
			//	SchemaName = schemaName
			//};

			// Sadece sizin tablonuzda yer alan sütunları kullanacağız.
			var columnOptions = new ColumnOptions();
			// Varsayılan sütunları kaldırıyoruz.
			columnOptions.Store.Clear();
			// Sadece ek sütunlar tanımlıyoruz.
			columnOptions.AdditionalColumns = new List<SqlColumn>
				{
					new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "ParcaState", DataLength = 200 },
					new SqlColumn { DataType = SqlDbType.BigInt, ColumnName = "ParcaPartID" },
					new SqlColumn { DataType = SqlDbType.BigInt, ColumnName = "ParcaPartMasterID" },
					new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "ParcaName", DataLength = 150 },
					new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "ParcaNumber", DataLength = -1 }, // -1 = MAX
                    new SqlColumn { DataType = SqlDbType.NChar, ColumnName = "ParcaVersion", DataLength = 30 },
					new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "KulAd", DataLength = 50 },
					new SqlColumn { DataType = SqlDbType.DateTime, ColumnName = "LogDate" },
					new SqlColumn { DataType = SqlDbType.TinyInt, ColumnName = "EntegrasyonDurum" },
					new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "LogMesaj", DataLength = 300 },
					new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "ActionType", DataLength = 50 },
					new SqlColumn { DataType = SqlDbType.DateTime, ColumnName = "ActionDate" }
				};

			// SelfLog'u etkinleştirerek, arka plandaki hataları görebilirsiniz (test amacıyla)
			// veya konsola:
			// Serilog.Debugging.SelfLog.Enable(Console.Error);

			// Logger'ı oluşturuyoruz: İki farklı sink çağrısı ekleyerek log eventlerini iki tabloya gönderiyoruz.
			//Log.Logger = new LoggerConfiguration()
			//	.WriteTo.MSSqlServer(connectionString, sinkOptionsLog, columnOptions: columnOptions)
			//	//.WriteTo.MSSqlServer(connectionString, sinkOptionsSent, columnOptions: columnOptions)
			//	.CreateLogger();
			//Logger = Log.Logger;

			Logger = new LoggerConfiguration()
			.WriteTo.MSSqlServer(connectionString, sinkOptionsLog, columnOptions: columnOptions)
			.CreateLogger();

		}
		catch (Exception ex)
		{
			throw new Exception("WTPartMsSqlLogger oluşturulurken hata meydana geldi.", ex);
		}
	}
}