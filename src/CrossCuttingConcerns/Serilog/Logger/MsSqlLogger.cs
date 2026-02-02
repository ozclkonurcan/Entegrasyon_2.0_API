using CrossCuttingConcerns.Serilog.ConfigurationModels;
using CrossCuttingConcerns.Serilog.Messages;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossCuttingConcerns.Serilog.Logger;

public class MsSqlLogger : LoggerServiceBase
{
	public MsSqlLogger(IConfiguration configuration)
	{
		try
		{

	
		Env.Load();

		MsSqlConfiguration logConfiguration =
			configuration.GetSection("SeriLogConfigurations:MsSqlConfiguration").Get<MsSqlConfiguration>()
			?? throw new Exception(SerilogMessages.NullOptionsMessage);

		MSSqlServerSinkOptions sinkOptions = new()
		{
			TableName = logConfiguration.TableName,
			AutoCreateSqlTable = logConfiguration.AutoCreateSqlTable,
			SchemaName = Env.GetString("SQL_SCHEMA")
		};


		string connectionString = Env.GetString("SQL_CONNECTION_STRING_ADRESS");

		if (string.IsNullOrEmpty(connectionString))
		{
			throw new Exception("SQL_CONNECTION_STRING_ADRESS environment variable not found.");
		}

		logConfiguration.ConnectionString = connectionString;

			ColumnOptions columnOptions = new();
			columnOptions.AdditionalColumns = new List<SqlColumn>
{
	new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "TetiklenenFonksiyon" },
	new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "KullaniciAdi" },
	new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "HataMesaji" },
};
			global::Serilog.Core.Logger seriLogConfig = new LoggerConfiguration().WriteTo
			.MSSqlServer(logConfiguration.ConnectionString, sinkOptions, columnOptions: columnOptions)
			.CreateLogger();

		Logger = seriLogConfig;
		}
		catch (Exception ex)
		{

			throw;
		}
	}
}

