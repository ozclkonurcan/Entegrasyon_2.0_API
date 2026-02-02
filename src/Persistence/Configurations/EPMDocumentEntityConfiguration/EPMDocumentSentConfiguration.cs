using Domain.Entities.EPMModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.EPMDocumentEntityConfiguration;

public class EPMDocumentSentConfiguration : IEntityTypeConfiguration<EPMDocument_SENT>
{
	public void Configure(EntityTypeBuilder<EPMDocument_SENT> builder)
	{
		// SQL'deki Tablo İsmi (Bunu kontrol et, genelde _Sent olur):
		builder.ToTable("Des2_EPMDocument_Sent").HasKey(b => b.Ent_ID);

		builder.Property(b => b.EPMDocID).HasColumnName("EPMDocID").IsRequired();
		builder.Property(b => b.StateDegeri).HasColumnName("StateDegeri").IsRequired();
		builder.Property(b => b.idA3masterReference).HasColumnName("idA3masterReference").IsRequired();
		builder.Property(b => b.CadName).HasColumnName("CadName").IsRequired();
		builder.Property(b => b.name).HasColumnName("name").IsRequired();
		builder.Property(b => b.docNumber).HasColumnName("docNumber").IsRequired();
	}
}