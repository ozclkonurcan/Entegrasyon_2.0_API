using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.WTPartEntityConfiguration;

public class WTPartAllLogsConfiguration : IEntityTypeConfiguration<WTPartAllLogs>
{
	public void Configure(EntityTypeBuilder<WTPartAllLogs> builder)
	{
		builder.ToTable("Des2_WTPart_Log").HasKey(b => b.LogID);
		builder.Property(b => b.LogID).HasColumnName("LogID").IsRequired();
		builder.Property(b => b.EntegrasyonDurum).HasColumnName("EntegrasyonDurum");
		builder.Property(b => b.ParcaState).HasColumnName("ParcaState").IsRequired(false);
		builder.Property(b => b.ParcaNumber).HasColumnName("ParcaNumber").IsRequired(false);
		builder.Property(b => b.ParcaName).HasColumnName("ParcaName").IsRequired(false);
		builder.Property(b => b.ParcaPartID).HasColumnName("ParcaPartID").IsRequired(false);      // false yap
		builder.Property(b => b.ParcaPartMasterID).HasColumnName("ParcaPartMasterID").IsRequired(false); // false yap
		builder.Property(b => b.ParcaVersion).HasColumnName("ParcaVersion").IsRequired(false);
		builder.Property(b => b.KulAd).HasColumnName("KulAd").IsRequired(false);
		builder.Property(b => b.LogDate).HasColumnName("LogDate").IsRequired(false);
		builder.Property(b => b.LogMesaj).HasColumnName("LogMesaj").IsRequired(false);

		builder.HasIndex(indexExpression: b => b.ParcaNumber, name: "UK_WTParts_ParcaNumber").IsUnique();
	}
}