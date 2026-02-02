using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.EPMDocumentEntityConfiguration;

public class EPMDocumentCancelledConfiguration : IEntityTypeConfiguration<EPMDocument_CANCELLED>
{
	public void Configure(EntityTypeBuilder<EPMDocument_CANCELLED> builder)
	{
		builder.ToTable("Des2_EPMDocument_Cancelled").HasKey(b => b.Ent_ID);
		builder.Property(b => b.EPMDocID).HasColumnName("EPMDocID").IsRequired();
		builder.Property(b => b.StateDegeri).HasColumnName("StateDegeri").IsRequired();
		builder.Property(b => b.idA3masterReference).HasColumnName("idA3masterReference").IsRequired();
		builder.Property(b => b.CadName).HasColumnName("CadName").IsRequired();
		builder.Property(b => b.name).HasColumnName("name").IsRequired();
		builder.Property(b => b.docNumber).HasColumnName("docNumber").IsRequired();


		builder.HasIndex(indexExpression: b => b.docNumber, name: "PK_EPMDocument_docNumber").IsUnique();

	}
}
