using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class BaseEntities
{

	public DateTime CreatedDate { get; set; }
	public DateTime? UpdatedDate { get; set; }
	public DateTime? DeletedDate { get; set; }



}
