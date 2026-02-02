using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Pipelines.WTPartLogging.WTPartAlternateLogging;

public interface IWTPartAlternateLoggableRequest
{
	public int LogID { get;  }
	public string AnaParcaState { get;  }
	public long AnaParcaPartID { get;  }
	public long AnaParcaPartMasterID { get;  }
	public string AnaParcaName { get;  }
	public string AnaParcaNumber { get;  }
	public string AnaParcaVersion { get;  }
	public string MuadilParcaState { get;  }
	public long MuadilParcaPartID { get;  }
	public long MuadilParcaMasterID { get;  }
	public string MuadilParcaName { get;  }
	public string MuadilParcaNumber { get;  }
	public string MuadilParcaVersion { get;  }
	public string KulAd { get;  }
	public string LogMesaj { get;  }
	public DateTime? LogDate { get;  }
	public byte? EntegrasyonDurum { get;  }
}
