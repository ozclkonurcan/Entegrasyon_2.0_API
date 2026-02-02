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

public class WTViewConfiguration : IEntityTypeConfiguration<WTView>
{
	public void Configure(EntityTypeBuilder<WTView> builder)
	{
		builder.ToTable("WTView").HasKey(b => b.idA2A2);
		builder.Property(b => b.idA2A2).HasColumnName("idA2A2").IsRequired();
		builder.Property(b => b.name).HasColumnName("name").IsRequired();
	}
}
