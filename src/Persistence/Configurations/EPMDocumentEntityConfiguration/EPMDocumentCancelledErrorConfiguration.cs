using Domain.Entities.EPMModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations.EPMDocumentEntityConfiguration;

public class EPMDocumentCancelledErrorConfiguration : IEntityTypeConfiguration<EPMDocument_CANCELLED_ERROR>
{
	public void Configure(EntityTypeBuilder<EPMDocument_CANCELLED_ERROR> builder)
	{
		// SQL Tablo Adı: Des2_EPMDocument_Cancelled_Error
		builder.ToTable("Des2_EPMDocument_Cancelled_Error").HasKey(b => b.Ent_ID);

		builder.Property(b => b.EPMDocID).HasColumnName("EPMDocID").IsRequired();
		builder.Property(b => b.StateDegeri).HasColumnName("StateDegeri");
		builder.Property(b => b.idA3masterReference).HasColumnName("idA3masterReference");
		builder.Property(b => b.CadName).HasColumnName("CadName");
		builder.Property(b => b.name).HasColumnName("name");
		builder.Property(b => b.docNumber).HasColumnName("docNumber");

		// Retry Sütunları
		builder.Property(b => b.LogMesaj).HasColumnName("LogMesaj");
		builder.Property(b => b.LogDate).HasColumnName("LogDate");
		builder.Property(b => b.EntegrasyonDurum).HasColumnName("EntegrasyonDurum");
		builder.Property(b => b.RetryCount).HasColumnName("RetryCount").HasDefaultValue(0);
		builder.Property(b => b.ActionType).HasColumnName("ActionType");
		builder.Property(b => b.LastRetryDate).HasColumnName("LastRetryDate");
	}
}