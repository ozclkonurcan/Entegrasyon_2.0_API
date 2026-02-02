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

namespace CrossCuttingConcerns.Serilog.Logger;

public class WTPartAlternateMsSqlLogger : LoggerServiceBase
{
	public WTPartAlternateMsSqlLogger(IConfiguration configuration)
	{
		try
		{
			// .env dosyasını yükleyin
			Env.Load();

			// Bağlantı dizesi ve şema bilgisi ortam değişkenlerinden alınıyor
			string connectionString = Env.GetString("SQL_CONNECTION_STRING_ADRESS");
			if (string.IsNullOrEmpty(connectionString))
				throw new Exception("Connection string bilgisi eksik.");

			string schemaName = Env.GetString("SQL_SCHEMA");
			if (string.IsNullOrEmpty(schemaName))
				throw new Exception("SQL schema bilgisi eksik.");

			// Tablo adları
			string tableNameLog = "Des2_WTPart_AlternateLink_Log";
			//string tableNameSent = "Des2_WTPart_AlternateLink_Sent";

			// Sink ayarları
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

			// Sütun ayarları
			var columnOptions = new ColumnOptions();
			columnOptions.Store.Clear();

			// Alternate Link için özel sütunlar
			columnOptions.AdditionalColumns = new List<SqlColumn>
			{
				new SqlColumn { DataType = SqlDbType.Int, ColumnName = "LogID" },
				new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "AnaParcaState", DataLength = 200 },
				new SqlColumn { DataType = SqlDbType.BigInt, ColumnName = "AnaParcaPartID" },
				new SqlColumn { DataType = SqlDbType.BigInt, ColumnName = "AnaParcaPartMasterID" },
				new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "AnaParcaName", DataLength = 150 },
				new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "AnaParcaNumber", DataLength = -1 },
				new SqlColumn { DataType = SqlDbType.NChar, ColumnName = "AnaParcaVersion", DataLength = 30 },
				new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "MuadilParcaState", DataLength = 200 },
				new SqlColumn { DataType = SqlDbType.BigInt, ColumnName = "MuadilParcaPartID" },
				new SqlColumn { DataType = SqlDbType.BigInt, ColumnName = "MuadilParcaMasterID" },
				new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "MuadilParcaName", DataLength = 150 },
				new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "MuadilParcaNumber", DataLength = -1 },
				new SqlColumn { DataType = SqlDbType.NChar, ColumnName = "MuadilParcaVersion", DataLength = 30 },
				new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "KulAd", DataLength = 50 },
				new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "LogMesaj", DataLength = 300 },
				new SqlColumn { DataType = SqlDbType.DateTime, ColumnName = "LogDate" },
				new SqlColumn { DataType = SqlDbType.TinyInt, ColumnName = "EntegrasyonDurum" }
			};

			// Logger'ı oluşturuyoruz
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
			throw new Exception("WTPartAlternateMsSqlLogger oluşturulurken hata meydana geldi.", ex);
		}
	}
}