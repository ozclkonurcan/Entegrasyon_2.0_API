using Domain.Entities.IntegrationSettings;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.IntegrationSettings;

public class RoleMappingEndpointConfiguration : IEntityTypeConfiguration<RoleMappingEndpoint>
{
	public void Configure(EntityTypeBuilder<RoleMappingEndpoint> builder)
	{
		builder.ToTable("Des2_RolAyarlari_Endpoints");
		builder.HasKey(e => e.Id);

		builder.Property(e => e.Id)
			   .HasColumnName("EndpointID")
			   .IsRequired();

		builder.Property(e => e.RoleMappingId)
			   .HasColumnName("RoleMappingID")
			   .IsRequired();

		builder.Property(e => e.TargetApi)
			   .HasColumnName("TargetApi")
			   .HasMaxLength(1000)
			   .IsRequired();

		builder.Property(e => e.Endpoint)
			   .HasColumnName("Endpoint")
			   .HasMaxLength(200);

		builder.Property(e => e.IsActive)
			   .HasColumnName("IsActive")
			   .IsRequired();
	}
}