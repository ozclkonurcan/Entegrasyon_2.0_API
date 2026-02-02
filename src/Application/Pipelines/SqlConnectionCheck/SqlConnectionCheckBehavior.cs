using DotNetEnv;
using MediatR;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Pipelines.SqlConnectionCheck
{
	public class SqlConnectionCheckBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
where TRequest : IRequest<TResponse>, ISqlConnectionCheckRequest
	{

		private readonly string _connectionString;

		public SqlConnectionCheckBehavior()
		{
			Env.Load();
			 _connectionString = Env.GetString("SQL_CONNECTION_STRING_ADRESS");
		}

		public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
		{
			if (!await CheckSqlConnectionAsync())
			{
				Console.WriteLine("SQL bağlantısı sağlanamadı. İstek işlenemedi.");
				throw new Exception("Veritabanı bağlantısı sağlanamadı.");
			}

			// Eğer bağlantı sağlanıyorsa, sonraki adıma geç
			Console.WriteLine("SQL bağlantısı sağlandı. İstek işleniyor.");
			return await next();
		}

		private async Task<bool> CheckSqlConnectionAsync()
		{
			try
			{
				using (var connection = new SqlConnection(_connectionString))
				{
					await connection.OpenAsync(); // Asenkron bağlantı açma
					return true;
				}
			}
			catch
			{
				return false;
			}
		}
	}
}
