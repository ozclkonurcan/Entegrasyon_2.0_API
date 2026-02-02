using Domain.Entities.EPMModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.EPMDocumentEntityConfiguration;

public class EPMDocumentErrorConfiguration : IEntityTypeConfiguration<EPMDocument_ERROR>
{
	public void Configure(EntityTypeBuilder<EPMDocument_ERROR> builder)
	{
		builder.ToTable("Des2_EPMDocument_Error").HasKey(b => b.Ent_ID);

		// Eski alanlar
		builder.Property(b => b.EPMDocID).HasColumnName("EPMDocID").IsRequired();
		builder.Property(b => b.StateDegeri).HasColumnName("StateDegeri");
		builder.Property(b => b.idA3masterReference).HasColumnName("idA3masterReference");
		builder.Property(b => b.CadName).HasColumnName("CadName");
		builder.Property(b => b.name).HasColumnName("name");
		builder.Property(b => b.docNumber).HasColumnName("docNumber");

		// YENİ EKLENENLERİN MAPPING'İ
		builder.Property(b => b.LogMesaj).HasColumnName("LogMesaj");
		builder.Property(b => b.LogDate).HasColumnName("LogDate");
		builder.Property(b => b.EntegrasyonDurum).HasColumnName("EntegrasyonDurum");
		builder.Property(b => b.RetryCount).HasColumnName("RetryCount").HasDefaultValue(0);
		builder.Property(b => b.ActionType).HasColumnName("ActionType");
		builder.Property(b => b.LastRetryDate).HasColumnName("LastRetryDate");
	}
}