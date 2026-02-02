using Domain.Entities;
using Domain.Entities.WTPartModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.WTPartEntityConfiguration;

internal class WTPart_Sql_Configuration : IEntityTypeConfiguration<WTPart_Sql>
{
	public void Configure(EntityTypeBuilder<WTPart_Sql> builder)
	{
		builder.ToTable("WTPart").HasKey(b => b.idA2A2);
		builder.Property(b => b.idA2A2).HasColumnName("idA2A2").IsRequired();
		builder.Property(b => b.idA3masterReference).HasColumnName("idA3masterReference").IsRequired();
		builder.Property(b => b.idA3View).HasColumnName("idA3View").IsRequired();
		builder.Property(b => b.latestiterationInfo).HasColumnName("latestiterationInfo").IsRequired();
		builder.Property(b => b.statestate).HasColumnName("statestate").IsRequired();
		builder.Property(b => b.branchIditerationInfo).HasColumnName("branchIditerationInfo").IsRequired();

	}
}
