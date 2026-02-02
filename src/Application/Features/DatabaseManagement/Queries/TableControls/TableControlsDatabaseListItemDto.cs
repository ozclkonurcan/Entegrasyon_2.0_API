using Domain.Entities.DatabaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.DatabaseManagement.Queries.TableControls;

public class TableControlsDatabaseListItemDto
{
	public string TableTitle { get; set; }
	public string TableName { get; set; }
	public string TableSchema { get; set; }
	public string CreateQuery { get; set; }
	public bool IsActive { get; set; }
	public List<TriggersDefinations>? Triggers { get; set; }

	public TableControlsDatabaseListItemDto()
	{

	}

	public TableControlsDatabaseListItemDto(string tableTitle, string tableName, string tableSchema, string createQuery, bool ısActive, List<TriggersDefinations> triggers)
	{
		TableTitle = tableTitle;
		TableName = tableName;
		TableSchema = tableSchema;
		CreateQuery = createQuery;
		IsActive = ısActive;
		Triggers = triggers;
	}
}



