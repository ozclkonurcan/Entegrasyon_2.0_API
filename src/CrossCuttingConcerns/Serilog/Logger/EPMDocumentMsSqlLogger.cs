using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Data;

namespace CrossCuttingConcerns.Serilog.Logger;

public class EPMDocumentMsSqlLogger : LoggerServiceBase
{
	public EPMDocumentMsSqlLogger(IConfiguration configuration)
	{
		try
		{
			Env.Load();

			string connectionString = Env.GetString("SQL_CONNECTION_STRING_ADRESS");
			if (string.IsNullOrEmpty(connectionString))
				throw new Exception("Connection string bilgisi eksik.");

			string schemaName = Env.GetString("SQL_SCHEMA");
			if (string.IsNullOrEmpty(schemaName))
				throw new Exception("SQL schema bilgisi eksik.");

			// TABLO ADI: EPMDocument için log tablosu
			string tableNameLog = "Des2_EPMDocument_Log";

			MSSqlServerSinkOptions sinkOptionsLog = new MSSqlServerSinkOptions
			{
				TableName = tableNameLog,
				AutoCreateSqlTable = false, // SQL'de elle oluşturacağız
				SchemaName = schemaName
			};

			var columnOptions = new ColumnOptions();
			columnOptions.Store.Clear(); // Standart Serilog sütunlarını siliyoruz

			// EPMDocument İÇİN ÖZEL SÜTUNLAR
			columnOptions.AdditionalColumns = new List<SqlColumn>
			{
                // EPMDocument verileri
                new SqlColumn { DataType = SqlDbType.BigInt, ColumnName = "EPMDocID" },
				new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "DocNumber", DataLength = 200 },
				new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "CadName", DataLength = 200 },
				new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "StateDegeri", DataLength = 50 },

                // Ortak Log verileri (WTPart ile aynı)
                new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "KulAd", DataLength = 50 },
				new SqlColumn { DataType = SqlDbType.DateTime, ColumnName = "LogDate" },
				new SqlColumn { DataType = SqlDbType.TinyInt, ColumnName = "EntegrasyonDurum" }, // 1: Başarılı, 2: Hatalı
                new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "LogMesaj", DataLength = -1 } // -1 = MAX (Hata mesajları uzun olabilir)
            };

			Logger = new LoggerConfiguration()
				.WriteTo.MSSqlServer(connectionString, sinkOptionsLog, columnOptions: columnOptions)
				.CreateLogger();
		}
		catch (Exception ex)
		{
			throw new Exception("EPMDocumentMsSqlLogger oluşturulurken hata meydana geldi.", ex);
		}
	}
}