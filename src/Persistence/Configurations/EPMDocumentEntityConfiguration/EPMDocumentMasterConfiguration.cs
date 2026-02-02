using Domain.Entities;
using Domain.Entities.EPMModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.EPMDocumentEntityConfiguration;

public class EPMDocumentMasterConfiguration : IEntityTypeConfiguration<EPMDocumentMaster>
{
	public void Configure(EntityTypeBuilder<EPMDocumentMaster> builder)
	{
		builder.ToTable("EPMDocumentMaster").HasKey(b => b.idA2A2);
		builder.Property(b => b.idA2A2).HasColumnName("idA2A2").IsRequired();
		builder.Property(b => b.CADName).HasColumnName("CADName").IsRequired();
		builder.Property(b => b.name).HasColumnName("name").IsRequired();
		builder.Property(b => b.documentNumber).HasColumnName("documentNumber").IsRequired();

	}
}
