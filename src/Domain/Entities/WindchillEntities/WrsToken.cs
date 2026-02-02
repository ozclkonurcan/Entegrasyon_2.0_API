using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.WindchillEntities;

public class WrsToken
{

	public string NonceKey { get; set; }
	public string NonceValue { get; set; }

	public WrsToken()
	{
		
	}

	public WrsToken(string nonceKey, string nonceValue)
	{
		NonceKey = nonceKey;
		NonceValue = nonceValue;
	}

}
