using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.IntegrationSettings;

namespace Persistence.Configurations.IntegrationSettings;

public class RoleProcessTagConfiguration : IEntityTypeConfiguration<RoleProcessTag>
{
	public void Configure(EntityTypeBuilder<RoleProcessTag> builder)
	{
		builder.ToTable("Des2_RolProcessTags");
		builder.HasKey(p => p.ProcessTagID);

		builder.Property(p => p.ProcessTagID)
			   .HasColumnName("ProcessTagID")
			   .IsRequired();

		builder.Property(p => p.TagName)
			   .HasColumnName("TagName")
			   .HasMaxLength(100)
			   .IsRequired();

		// Varsayılan Verileri Ekleyelim
		builder.HasData(
			new RoleProcessTag { ProcessTagID = 1, TagName = "WTPART" },
			new RoleProcessTag { ProcessTagID = 2, TagName = "EPMDocument" }
		);
	}
}