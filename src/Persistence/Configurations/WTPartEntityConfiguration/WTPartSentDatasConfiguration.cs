using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.WTPartEntityConfiguration;

public class WTPartSentDatasConfiguration : IEntityTypeConfiguration<WTPartSentDatas>
{
	public void Configure(EntityTypeBuilder<WTPartSentDatas> builder)
	{
		builder.ToTable("Des2_WTPart_Sent").HasKey(b => b.LogID);
		//builder.ToTable("Des_WTPart_LogTable").HasKey(b => b.LogID);
		builder.Property(b => b.LogID).HasColumnName("LogID").IsRequired();
		builder.Property(b => b.EntegrasyonDurum).HasColumnName("EntegrasyonDurum");
		builder.Property(b => b.ParcaState).HasColumnName("ParcaState").IsRequired();
		builder.Property(b => b.ParcaNumber).HasColumnName("ParcaNumber").IsRequired();
		builder.Property(b => b.ParcaName).HasColumnName("ParcaName").IsRequired();
		builder.Property(b => b.ParcaPartID).HasColumnName("ParcaPartID");
		builder.Property(b => b.ParcaPartMasterID).HasColumnName("ParcaPartMasterID");
		builder.Property(b => b.ParcaVersion).HasColumnName("ParcaVersion").IsRequired();
		builder.Property(b => b.KulAd).HasColumnName("KulAd");
		builder.Property(b => b.LogDate).HasColumnName("LogDate");
		builder.Property(b => b.LogMesaj).HasColumnName("LogMesaj");

		builder.HasIndex(indexExpression: b => b.ParcaNumber, name: "UK_WTParts_ParcaNumber").IsUnique();
		//builder.HasMany(b => b.Models);

		//builder.HasQueryFilter(b => !b.DeletedDate.HasValue);
	}
}
