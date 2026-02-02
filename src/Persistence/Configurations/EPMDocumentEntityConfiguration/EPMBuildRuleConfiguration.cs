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

public class EPMBuildRuleConfiguration : IEntityTypeConfiguration<EPMBuildRule>
{
	public void Configure(EntityTypeBuilder<EPMBuildRule> builder)
	{
		builder.ToTable("EPMBuildRule").HasKey(b => b.idA2A2);
		builder.Property(b => b.idA2A2).HasColumnName("idA2A2").IsRequired();
		builder.Property(b => b.branchIdA3B5).HasColumnName("branchIdA3B5").IsRequired();

	}
}
