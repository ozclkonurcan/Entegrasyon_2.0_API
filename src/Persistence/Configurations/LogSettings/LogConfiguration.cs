using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.LogSettings;

namespace Persistence.Configurations.LogSettings;

public class LogConfiguration : IEntityTypeConfiguration<LogEntry>
{
	public void Configure(EntityTypeBuilder<LogEntry> builder)
	{
		builder.ToTable("Logs").HasKey(u => u.Id);

		builder.Property(u => u.Id).HasColumnName("Id").IsRequired();
		builder.Property(u => u.Message).HasColumnName("Message").IsRequired();
		builder.Property(u => u.MessageTemplate).HasColumnName("MessageTemplate").IsRequired();
		builder.Property(u => u.Level).HasColumnName("Level").IsRequired();
		builder.Property(u => u.TimeStamp).HasColumnName("TimeStamp").IsRequired();
		builder.Property(u => u.Exception).HasColumnName("Exception");
		builder.Property(u => u.Properties).HasColumnName("Properties");
		builder.Property(u => u.TetiklenenFonksiyon).HasColumnName("TetiklenenFonksiyon");
		builder.Property(u => u.KullaniciAdi).HasColumnName("KullaniciAdi");
		builder.Property(u => u.HataMesaji).HasColumnName("HataMesaji");

	}
}