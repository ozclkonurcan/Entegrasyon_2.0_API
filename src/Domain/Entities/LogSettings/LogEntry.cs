using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.LogSettings;

public class LogEntry
{
	[Key]
	public int Id { get; set; }
	public string? Message { get; set; }
	public string? MessageTemplate { get; set; }
	public string? Level { get; set; }
	public DateTime TimeStamp { get; set; }
	public string? Exception { get; set; }
	public string? Properties { get; set; }
	public string? TetiklenenFonksiyon { get; set; }
	public string? KullaniciAdi { get; set; }
	public string? HataMesaji { get; set; }
}
