using Domain.Entities.EPMModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations.EPMDocumentEntityConfiguration;

public class EPMDocumentCancelledSentConfiguration : IEntityTypeConfiguration<EPMDocument_CANCELLED_SENT>
{
	public void Configure(EntityTypeBuilder<EPMDocument_CANCELLED_SENT> builder)
	{
		// SQL Tablo Adı: Des2_EPMDocument_Cancelled_Sent
		builder.ToTable("Des2_EPMDocument_Cancelled_Sent").HasKey(b => b.Ent_ID);

		builder.Property(b => b.EPMDocID).HasColumnName("EPMDocID").IsRequired();
		builder.Property(b => b.StateDegeri).HasColumnName("StateDegeri");
		builder.Property(b => b.idA3masterReference).HasColumnName("idA3masterReference");
		builder.Property(b => b.CadName).HasColumnName("CadName");
		builder.Property(b => b.name).HasColumnName("name");
		builder.Property(b => b.docNumber).HasColumnName("docNumber");

		// Eğer ProcessDate varsa:
		// builder.Property(b => b.ProcessDate).HasColumnName("ProcessDate");
	}
}