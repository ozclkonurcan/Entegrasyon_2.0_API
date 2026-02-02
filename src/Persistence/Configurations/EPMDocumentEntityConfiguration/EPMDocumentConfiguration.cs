using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.EPMDocumentEntityConfiguration;

public class EPMDocumentConfiguration : IEntityTypeConfiguration<EPMDocument>
{
	public void Configure(EntityTypeBuilder<EPMDocument> builder)
	{
		builder.ToTable("EPMDocument").HasKey(b => b.idA2A2);
		builder.Property(b => b.statecheckoutInfo).HasColumnName("statecheckoutInfo").IsRequired();
		builder.Property(b => b.statestate).HasColumnName("statestate").IsRequired();

		builder.Property(b => b.idA3masterReference).HasColumnName("idA3masterReference").IsRequired();
		builder.Property(b => b.latestiterationInfo).HasColumnName("latestiterationInfo").IsRequired();
		builder.Property(b => b.versionIdA2versionInfo).HasColumnName("versionIdA2versionInfo").IsRequired();
		builder.Property(b => b.versionLevelA2versionInfo).HasColumnName("versionLevelA2versionInfo").IsRequired();

	}
}
