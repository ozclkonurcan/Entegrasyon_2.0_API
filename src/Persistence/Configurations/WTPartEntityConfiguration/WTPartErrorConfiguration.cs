using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.WTPartEntityConfiguration;

public class WTPartErrorConfiguration : IEntityTypeConfiguration<WTPartError>
{
	public void Configure(EntityTypeBuilder<WTPartError> builder)
	{
		builder.ToTable("Des2_WTPart_Error").HasKey(b => b.ErrorID);
		//builder.ToTable("Des_WTPart_LogTable").HasKey(b => b.LogID);
		builder.Property(b => b.ErrorID).HasColumnName("ErrorID").IsRequired();
		builder.Property(b => b.LogID).HasColumnName("LogID");
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
		builder.Property(b => b.ErrorMessage).HasColumnName("ErrorMessage");
		builder.Property(b => b.ErrorDate).HasColumnName("ErrorDate");
		builder.Property(b => b.RetryCount).HasColumnName("DenemeSayisi");
		builder.Property(b => b.LastRetryDate).HasColumnName("DenemeTarihi");

		builder.HasIndex(indexExpression: b => b.ParcaNumber, name: "UK_WTParts_ParcaNumber").IsUnique();
		//builder.HasMany(b => b.Models);

		//builder.HasQueryFilter(b => !b.DeletedDate.HasValue);
	}
}
