using Domain.Entities.WTPartModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.WTPartEntityConfiguration;

public class WTPartMaster_Sql_Configuration : IEntityTypeConfiguration<WTPartMaster_Sql>
{
	public void Configure(EntityTypeBuilder<WTPartMaster_Sql> builder)
	{
		builder.ToTable("WTPartMaster").HasKey(b => b.idA2A2);
		builder.Property(b => b.idA2A2).HasColumnName("idA2A2").IsRequired();
		builder.Property(b => b.WTPartNumber).HasColumnName("WTPartNumber").IsRequired();
		builder.Property(b => b.name).HasColumnName("name").IsRequired();

	}
}
