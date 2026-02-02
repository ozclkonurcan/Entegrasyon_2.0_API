using Application.Features.Connection.Sql.Queries.SqlContorls;
using Application.Interfaces.ConnectionModule;
using Domain.Entities;
using DotNetEnv;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories.ConnectionModule;

public class ConnectionRepository : IConnectionService
{
	public async Task<bool> ConnectionControl()
	{
		try
		{
			// .env dosyasındaki bağlantı bilgilerini yükle
			Env.Load();

			// Bağlantı bilgilerini al
			string connectionString = Env.GetString("SQL_CONNECTION_STRING_ADRESS");

			// SqlConnection nesnesi oluştur
			using (var connection = new SqlConnection(connectionString))
			{
				// Bağlantıyı açmaya çalış
				await connection.OpenAsync();

				// Bağlantı başarılı ise true döndür
				return true;
			}
		}
		catch (Exception ex)
		{
			// Bağlantı başarısız ise hata mesajını logla ve false döndür
			Console.WriteLine($"Bağlantı hatası: {ex.Message}");
			return false;
		}
	}

	public async Task<bool> ConnectionControlWithModel(ConnectionSettings model)
	{
		try
		{
			// .env dosyasındaki bağlantı bilgilerini yükle
			Env.Load();

			// Bağlantı bilgilerini al
			string connectionString = $"Persist Security Info=False; Server={model.Server};Initial Catalog={model.Database};User Id={model.Username};Password={model.Password};TrustServerCertificate=True;Connection Timeout=60;Command Timeout=300";

			// SqlConnection nesnesi oluştur
			using (var connection = new SqlConnection(connectionString))
			{
				// Bağlantıyı açmaya çalış
				await connection.OpenAsync();

				// Bağlantı başarılı ise true döndür
				return true;
			}
		}
		catch (Exception ex)
		{
			// Bağlantı başarısız ise hata mesajını logla ve false döndür
			Console.WriteLine($"Bağlantı hatası: {ex.Message}");
			return false;
		}
	}

	public Task<ConnectionSettings> GetConnectionInformation()
	{
		Env.Load();

		string connectionFullURL = Env.GetString("SQL_CONNECTION_STRING_ADRESS");
		string connectionSqlServer = Env.GetString("SQL_SERVER");
		string connectionDatabase = Env.GetString("SQL_DATABASE");
		string connectionSchema = Env.GetString("SQL_SCHEMA");

		var connectionSettings = new ConnectionSettings
		{
			FullURL = connectionFullURL,
			Server = connectionSqlServer,
			Database = connectionDatabase,
			Schema = connectionSchema
		};

		return Task.FromResult(connectionSettings);
	}

	public async Task<ConnectionSettings> UpdateConnectionInformation(ConnectionSettings connectionSettings)
	{
		// .env dosyasının yolu
		string envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");

		// Dosyayı asenkron olarak oku
		string[] lines = await System.IO.File.ReadAllLinesAsync(envFile);

		// Dosyadaki satırları güncelle
		for (int i = 0; i < lines.Length; i++)
		{
			if (lines[i].StartsWith("SQL_SERVER="))
			{
				lines[i] = $"SQL_SERVER={connectionSettings.Server}";
			}
			else if (lines[i].StartsWith("SQL_DATABASE="))
			{
				lines[i] = $"SQL_DATABASE={connectionSettings.Database}";
			}
			else if (lines[i].StartsWith("SQL_USER="))
			{
				lines[i] = $"SQL_USER={connectionSettings.Username}";
			}
			else if (lines[i].StartsWith("SQL_PASSWORD="))
			{
				lines[i] = $"SQL_PASSWORD={connectionSettings.Password}";
			}
			else if (lines[i].StartsWith("SQL_SCHEMA="))
			{
				lines[i] = $"SQL_SCHEMA={connectionSettings.Schema}";
			}
			else if (lines[i].StartsWith("SQL_CONNECTION_STRING_ADRESS="))
			{
				lines[i] = $"SQL_CONNECTION_STRING_ADRESS= Persist Security Info=False; Server={connectionSettings.Server};Initial Catalog={connectionSettings.Database};User Id={connectionSettings.Username};Password={connectionSettings.Password};TrustServerCertificate=True;Connection Timeout=60;Command Timeout=300";
			}
		}

		// Güncellenmiş satırları asenkron olarak yaz
		await System.IO.File.WriteAllLinesAsync(envFile, lines);

		// Güncellenmiş bağlantı bilgilerini döndür
		return connectionSettings;

	}
}
