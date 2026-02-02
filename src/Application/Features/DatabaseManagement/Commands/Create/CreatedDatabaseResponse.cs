using Domain.Entities.DatabaseManagement;
using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DatabaseManagement.Commands.Create;

public class CreatedDatabaseResponse
{


	public string TableTitle { get; set; }
	public string TableName { get; set; }
	public string TableSchema { get; set; }
	public string CreateQuery { get; set; }
	public bool IsActive { get; set; }
	public List<TriggersDefinations>? Triggers { get; set; }

	public CreatedDatabaseResponse(string tableTitle,string tableSchema, string tableName, string createQuery, bool ısActive, List<TriggersDefinations> triggers)
	{
		TableTitle = tableTitle;
		TableName = tableName;
		TableSchema = tableSchema;
		CreateQuery = createQuery;
		IsActive = ısActive;
		Triggers = triggers;
	}
}


