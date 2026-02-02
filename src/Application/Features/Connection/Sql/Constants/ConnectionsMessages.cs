using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Sql.Constants;

public class ConnectionsMessages
{
	public const string ConnectionAlreadyExists = "Sunucuyla bu bağlantı zaten mevcut";

	public const string ConnectionServerExists = "Bağlantı sunucusu mevcut";
	public const string ConnectionDatabaseExists = "Bağlantı veritabanı mevcut";
	public const string ConnectionUsernameExists = "Bağlantı kullanıcı adı mevcut";
	public const string ConnectionPasswordExists = "Bağlantı parolası mevcut";
	public const string ConnectionSchemaExists = "Bağlantı şeması mevcut";
}
