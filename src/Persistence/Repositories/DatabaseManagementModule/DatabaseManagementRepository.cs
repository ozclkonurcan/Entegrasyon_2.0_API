using Application.Interfaces.DatabaseManagementModule;
using Application.Paging;
using Application.Responses;
using Domain.Entities.DatabaseManagement;
using DotNetEnv;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories.DatabaseManagementModule;

public class DatabaseManagementRepository : IDatabaseManagementService
{

	public Task<DatabaseManagementDefinations> AddAsync(DatabaseManagementDefinations entity)
	{
		throw new NotImplementedException();
	}

	public Task<ICollection<DatabaseManagementDefinations>> AddRangeAsync(ICollection<DatabaseManagementDefinations> entities)
	{
		throw new NotImplementedException();
	}

	public Task<bool> AnyAsync(Expression<Func<DatabaseManagementDefinations, bool>>? predicate = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<DatabaseManagementDefinations> DeleteAsync(DatabaseManagementDefinations entity, bool permanent = false)
	{
		throw new NotImplementedException();
	}

	public Task<ICollection<DatabaseManagementDefinations>> DeleteRangeAsync(ICollection<DatabaseManagementDefinations> entities, bool permanent = false)
	{
		throw new NotImplementedException();
	}

	public Task<DatabaseManagementDefinations?> GetAsync(Expression<Func<DatabaseManagementDefinations, bool>> predicate, Func<IQueryable<DatabaseManagementDefinations>, IIncludableQueryable<DatabaseManagementDefinations, object>>? include = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<Paginate<DatabaseManagementDefinations>> GetListAsync(Expression<Func<DatabaseManagementDefinations, bool>>? predicate = null, Func<IQueryable<DatabaseManagementDefinations>, IOrderedQueryable<DatabaseManagementDefinations>>? orderBy = null, Func<IQueryable<DatabaseManagementDefinations>, IIncludableQueryable<DatabaseManagementDefinations, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IQueryable<DatabaseManagementDefinations> Query()
	{
		throw new NotImplementedException();
	}

	#region Özel Reposlar

	public async Task<GetListResponse<DatabaseManagementDefinations>> GetTablesAsync()
	{
		// JSON dosyasının yolunu belirle
		var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "DatabaseManagementJson", "DatabaseManagementSettings.json");

		// Dosya yoksa hata fırlat
		if (!File.Exists(jsonFilePath))
		{
			throw new FileNotFoundException("JSON dosyası bulunamadı.", jsonFilePath);
		}

		// JSON dosyasını oku
		var json = await File.ReadAllTextAsync(jsonFilePath);

		// JSON'u deserialize et
		var tableConfig = JsonConvert.DeserializeObject<DatabaseManagementSettings>(json);

		// Schema bilgisini dinamik olarak al (örneğin, environment'dan)
		var schema = Environment.GetEnvironmentVariable("SQL_SCHEMA") ?? "dbo";

		// DatabaseManagementDefinations listesine dönüştür
		var tableDefinitions = tableConfig.Tables.Select(t => new DatabaseManagementDefinations(
			t.TableTitle,
			t.TableName,
			schema,
			t.CreateQuery.Replace("{schema}", schema),
			t.IsActive,
			t.Triggers // or null or new List<TriggersDefinations>()
		)).ToList();

		// GetListResponse olarak döndür
		return new GetListResponse<DatabaseManagementDefinations>
		{
			Items = tableDefinitions,
			Count = tableDefinitions.Count
		};
	}


	#region SETUP

	public async Task<string?> SetupTablels()
	{
		Env.Load();
		var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "DatabaseManagementJson", "DatabaseManagementSettings.json");

		if (!File.Exists(jsonFilePath))
		{
			throw new FileNotFoundException("JSON dosyası bulunamadı.", jsonFilePath);
		}

		var json = await File.ReadAllTextAsync(jsonFilePath);
		var tableConfig = JsonConvert.DeserializeObject<DatabaseManagementSettings>(json);
		var schema = Environment.GetEnvironmentVariable("SQL_SCHEMA") ?? "dbo";

		var tables = tableConfig.Tables.Select(t => new DatabaseManagementDefinations(
			t.TableTitle,
			t.TableName,
			schema,
			t.CreateQuery.Replace("{schema}", schema),
			t.IsActive,
			t.Triggers
		)).ToList();

		var connectionString = Env.GetString("SQL_CONNECTION_STRING_ADRESS");

		await Parallel.ForEachAsync(tables, async (table, cancellationToken) =>
		{
			if (!string.IsNullOrEmpty(table.CreateQuery))
			{
				await CreateTableAsync(table, connectionString, schema);
			}

			if (table.Triggers != null && table.Triggers.Any())
			{
				await CreateTriggersAsync(table, connectionString, schema);
			}
		});

		return $"{tables.Count} tablonun kurulumu başarıyla tamamlandı.";
	}

	public async Task CreateTriggersAsync(DatabaseManagementDefinations tableDefinition, string connectionFullURL, string schema)
	{
		try
		{
			using (var connection = new SqlConnection(connectionFullURL))
			{
				await connection.OpenAsync();

				foreach (var trigger in tableDefinition.Triggers)
				{
					trigger.CreateQuery = trigger.CreateQuery.Replace("{schema}", schema);

					var cleanedTriggerQuery = trigger.CreateQuery
						.Replace("\r", " ")
						.Replace("\n", " ")
						.Replace("  ", " ");

					var triggerExistsQuery = $@"
                IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = '{trigger.TriggerName}' AND parent_id = OBJECT_ID('{schema}.{tableDefinition.TableName}'))
                BEGIN
                    EXEC('{cleanedTriggerQuery.Replace("'", "''")}');
                END";

					using (var triggerCommand = new SqlCommand(triggerExistsQuery, connection))
					{
						await triggerCommand.ExecuteNonQueryAsync();
					}
				}
			}
		}
		catch (SqlException sqlEx)
		{
			Console.WriteLine($"SQL Hatası: {sqlEx.Message}");
			throw new Exception("Tetikleyici oluşturulurken bir SQL hatası oluştu.", sqlEx);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Hata: {ex.Message}");
			throw new Exception("Tetikleyici oluşturulurken bir hata oluştu.", ex);
		}
	}


	public async Task CreateTableAsync(DatabaseManagementDefinations tableDefinition, string connectionFullURL, string schema)
	{
		try
		{
			using (var connection = new SqlConnection(connectionFullURL))
			{
				await connection.OpenAsync();

				var tableExistsQuery = $@"
            IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableDefinition.TableName}' AND TABLE_SCHEMA = '{schema}')
            BEGIN
                {tableDefinition.CreateQuery.Replace("{schema}", schema)}
            END";

				using (var command = new SqlCommand(tableExistsQuery, connection))
				{
					await command.ExecuteNonQueryAsync();
				}
			}
		}
		catch (SqlException sqlEx)
		{
			Console.WriteLine($"SQL Hatası: {sqlEx.Message}");
			throw new Exception("Tablo oluşturulurken bir SQL hatası oluştu.", sqlEx);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Hata: {ex.Message}");
			throw new Exception("Tablo oluşturulurken bir hata oluştu.", ex);
		}
	}
	#endregion




	//public async Task<string?> SetupTablels()
	//{


	//	Env.Load();
	//	// Tabloları al

	//	var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "DatabaseManagementJson", "DatabaseManagementSettings.json");

	//	// Dosya yoksa hata fırlat
	//	if (!File.Exists(jsonFilePath))
	//	{
	//		throw new FileNotFoundException("JSON dosyası bulunamadı.", jsonFilePath);
	//	}

	//	// JSON dosyasını oku
	//	var json = await File.ReadAllTextAsync(jsonFilePath);

	//	// JSON'u deserialize et
	//	var tableConfig = JsonConvert.DeserializeObject<DatabaseManagementSettings>(json);

	//	// Schema bilgisini dinamik olarak al (örneğin, environment'dan)
	//	var schema = Environment.GetEnvironmentVariable("SQL_SCHEMA") ?? "dbo";

	//	// DatabaseManagementDefinations listesine dönüştür
	//	var tables = tableConfig.Tables.Select(t => new DatabaseManagementDefinations(
	//		t.TableTitle,
	//		t.TableName,
	//		schema,
	//		t.CreateQuery.Replace("{schema}", schema),
	//		t.IsActive,
	//		t.Triggers
	//	)).ToList();

	//	// Bağlantı dizesini al
	//	var connectionString = Env.GetString("SQL_CONNECTION_STRING_ADRESS");

	//	// Paralel olarak tabloları oluştur
	//	await Parallel.ForEachAsync(tables, async (table, cancellationToken) =>
	//	{
	//		await CreateTableAsync(table, connectionString, schema);
	//	});

	//	return $"{tables.Count} tablonun kurulumu başarıyla tamamlandı.";
	//}

	public async Task<DatabaseManagementDefinations?> SetupModulerTables(string TableTitle)
	{
		throw new NotImplementedException();
	}


	public async Task<DatabaseManagementDefinations?> SetupTablels(string TableName)
	{
		throw new NotImplementedException();
	}

	public async Task<DatabaseManagementDefinations?> TableControlsAsync(DatabaseManagementDefinations databaseManagementDefinations)
	{
		// Veritabanı bağlantısı ve sorgu işlemleri için gerekli kodlar
		Env.Load();
		var connectionString = Env.GetString("SQL_CONNECTION_STRING_ADRESS");

		using (var connection = new SqlConnection(connectionString))
		{
			await connection.OpenAsync();

			// Tablonun var olup olmadığını kontrol etmek için sorgu
			var tableQuery = @"
            SELECT COUNT(*) 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_NAME = @TableName";

			using (var tableCommand = new SqlCommand(tableQuery, connection))
			{
				tableCommand.Parameters.AddWithValue("@TableName", databaseManagementDefinations.TableName);

				// Sorguyu çalıştır ve sonucu al
				var tableExists = (int)await tableCommand.ExecuteScalarAsync() > 0;

				if (tableExists)
				{
					// Tablo varsa, IsActive değerini true yap (varsayılan olarak)
					databaseManagementDefinations.IsActive = true;

					// Eğer tablo için trigger tanımlanmışsa, trigger'ları kontrol et
					var expectedTriggerNames = databaseManagementDefinations.Triggers?
						.Select(t => t.TriggerName)
						.ToList();

					if (expectedTriggerNames != null && expectedTriggerNames.Any())
					{
						// Her bir beklenen trigger için kontrol yap
						foreach (var expectedTriggerName in expectedTriggerNames)
						{
							var triggerQuery = @"
                            SELECT COUNT(*)
                            FROM sys.triggers
                            WHERE name = @TriggerName";

							using (var triggerCommand = new SqlCommand(triggerQuery, connection))
							{
								triggerCommand.Parameters.AddWithValue("@TriggerName", expectedTriggerName);

								// Sorguyu çalıştır ve sonucu al
								var triggerExists = (int)await triggerCommand.ExecuteScalarAsync() > 0;

								// Eğer beklenen trigger yoksa, IsActive false yap ve döngüden çık
								if (!triggerExists)
								{
									databaseManagementDefinations.IsActive = false;
									break;
								}
							}
						}
					}
				}
				else
				{
					// Tablo yoksa IsActive değerini false yap (veya varsayılan değeri koru)
					databaseManagementDefinations.IsActive = false;
				}
			}
		}

		return databaseManagementDefinations;
	}

	//Eski Kontrol
	//public async Task<DatabaseManagementDefinations?> TableControlsAsync(DatabaseManagementDefinations databaseManagementDefinations)
	//{


	//	// Veritabanı bağlantısı ve sorgu işlemleri için gerekli kodlar
	//	Env.Load();
	//	var connectionString = Env.GetString("SQL_CONNECTION_STRING_ADRESS");

	//	using (var connection = new SqlConnection(connectionString))
	//	{
	//		await connection.OpenAsync();

	//		// Tablonun var olup olmadığını kontrol etmek için sorgu
	//		var query = @"
	//           SELECT COUNT(*) 
	//           FROM INFORMATION_SCHEMA.TABLES 
	//           WHERE TABLE_NAME = @TableName";

	//		using (var command = new SqlCommand(query, connection))
	//		{
	//			command.Parameters.AddWithValue("@TableName", databaseManagementDefinations.TableName);

	//			// Sorguyu çalıştır ve sonucu al
	//			var tableExists = (int)await command.ExecuteScalarAsync() > 0;

	//			if (tableExists)
	//			{
	//				// Tablo varsa IsActive değerini true yap
	//				databaseManagementDefinations.IsActive = true;
	//			}
	//			else
	//			{
	//				// Tablo yoksa IsActive değerini false yap (veya varsayılan değeri koru)
	//				databaseManagementDefinations.IsActive = false;
	//			}
	//		}
	//	}

	//	return databaseManagementDefinations;
	//}


	#endregion




	public Task<DatabaseManagementDefinations> UpdateAsync(DatabaseManagementDefinations entity)
	{
		throw new NotImplementedException();
	}

	public Task<ICollection<DatabaseManagementDefinations>> UpdateRangeAsync(ICollection<DatabaseManagementDefinations> entities)
	{
		throw new NotImplementedException();
	}




	//public async Task CreateTableAsync(DatabaseManagementDefinations tableDefinition, string connectionFullURL, string schema)
	//{
	//	try
	//	{
	//		using (var connection = new SqlConnection(connectionFullURL))
	//		{
	//			await connection.OpenAsync();

	//			// Tablo var mı kontrol et ve oluştur
	//			var tableExistsQuery = $@"
 //               IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableDefinition.TableName}' AND TABLE_SCHEMA = '{schema}')
 //               BEGIN
 //                   {tableDefinition.CreateQuery.Replace("{schema}", schema)}
 //               END";

	//			using (var command = new SqlCommand(tableExistsQuery, connection))
	//			{
	//				await command.ExecuteNonQueryAsync();
	//			}

	//			// Tetikleyicileri kontrol et ve oluştur
	//			if (tableDefinition.Triggers != null && tableDefinition.Triggers.Any())
	//			{
	//				foreach (var trigger in tableDefinition.Triggers)
	//				{
	//					// {schema} ifadesini değiştir
	//					trigger.CreateQuery = trigger.CreateQuery.Replace("{schema}", schema);

	//					// Tetikleyici sorgusunu temizle
	//					var cleanedTriggerQuery = trigger.CreateQuery
	//						.Replace("\r", " ")  // Satır sonlarını boşlukla değiştir
	//						.Replace("\n", " ")  // Satır sonlarını boşlukla değiştir
	//						.Replace("  ", " "); // Çift boşlukları tek boşlukla değiştir

	//					// Tetikleyici var mı kontrol et
	//					var triggerExistsQuery = $@"
 //                       IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = '{trigger.TriggerName}' AND parent_id = OBJECT_ID('{schema}.{tableDefinition.TableName}'))
 //                       BEGIN
 //                           -- Tetikleyici yoksa, burada tetikleyiciyi oluştur
 //                           EXEC('{cleanedTriggerQuery.Replace("'", "''")}');
 //                       END";

	//					// Tetikleyici sorgusunu çalıştır
	//					using (var triggerCommand = new SqlCommand(triggerExistsQuery, connection))
	//					{
	//						await triggerCommand.ExecuteNonQueryAsync();
	//					}
	//				}
	//			}
	//		}
	//	}
	//	catch (SqlException sqlEx)
	//	{
	//		// SQL hatası durumunda detaylı bilgi al
	//		Console.WriteLine($"SQL Hatası: {sqlEx.Message}");
	//		throw new Exception("Tetikleyici oluşturulurken bir SQL hatası oluştu.", sqlEx);
	//	}
	//	catch (Exception ex)
	//	{
	//		// Genel hata durumunda
	//		Console.WriteLine($"Hata: {ex.Message}");
	//		throw new Exception("Tablo veya tetikleyici oluşturulurken bir hata oluştu.", ex);
	//	}
	//}


}

public class DatabaseManagementSettings
{
	public List<DatabaseManagementDefinationsSettings> Tables { get; set; }
}

public class DatabaseManagementDefinationsSettings
{
	public string TableTitle { get; set; }
	public string TableName { get; set; }
	public string CreateQuery { get; set; }
	public bool IsActive { get; set; }
	public List<TriggersDefinations>? Triggers { get; set; } // Triggers özelliğini ekle
}