using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.EPMReferenceLinkEntityConfiguration;

public class EPMReferenceLinkConfiguration : IEntityTypeConfiguration<EPMReferenceLink>
{
	public void Configure(EntityTypeBuilder<EPMReferenceLink> builder)
	{
		builder.ToTable("EPMReferenceLink").HasKey(b => b.idA2A2);
		builder.Property(b => b.idA2A2).HasColumnName("idA2A2").IsRequired();
		builder.Property(b => b.idA3B5).HasColumnName("idA3B5").IsRequired();

	}
}
