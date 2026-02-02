using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.IntegrationSettings;

namespace Persistence.Configurations.IntegrationSettings;

public class RoleMappingConfiguration : IEntityTypeConfiguration<RoleMapping>
{
	public void Configure(EntityTypeBuilder<RoleMapping> builder)
	{
		// RoleMapping tablosunun adı
		builder.ToTable("Des2_RolAyarlari");
		builder.HasKey(u => u.Id);

		builder.Property(u => u.Id)
			   .HasColumnName("RoleMappingID")
			   .IsRequired();

		builder.Property(u => u.RoleName)
			   .HasColumnName("RoleName")
			   .HasMaxLength(100)
			   .IsRequired();

		builder.Property(u => u.ProcessTagID) 
				   .HasColumnName("ProcessTagID")
				   .IsRequired();

		builder.Property(u => u.SourceApi)
			   .HasColumnName("SourceApi")
			   .HasMaxLength(200);

		builder.Property(u => u.IsActive)
			   .HasColumnName("IsActive")
			   .IsRequired();

		builder.HasMany(u => u.WindchillAttributes)
   .WithOne(a => a.RoleMapping)
   .HasForeignKey(a => a.RoleMappingId)
   .OnDelete(DeleteBehavior.Cascade);

		// ProcessTag ile ilişkilendir
		builder.HasOne(u => u.RoleProcessTag)
			   .WithMany(p => p.RoleMappings)
			   .HasForeignKey(u => u.ProcessTagID)
			   .OnDelete(DeleteBehavior.Restrict);

		// Endpoints koleksiyonu, ilgili RoleMappingEndpoint tablosu ile ilişkilendiriliyor.
		builder.HasMany(u => u.Endpoints)
			   .WithOne(e => e.RoleMapping)
			   .HasForeignKey(e => e.RoleMappingId)
			   .OnDelete(DeleteBehavior.Cascade);

	
	}

	//public void Configure(EntityTypeBuilder<RoleMapping> builder)
	//{
	//	builder.ToTable("Des2_RolAyarlari");
	//	builder.HasKey(u => u.Id);

	//	builder.Property(u => u.Id)
	//		   .HasColumnName("RoleMappingID")
	//		   .IsRequired();

	//	builder.Property(u => u.RoleName)
	//		   .HasColumnName("RoleName")
	//		   .HasMaxLength(100)
	//		   .IsRequired();

	//	builder.Property(u => u.SourceApi)
	//		   .HasColumnName("SourceApi")
	//		   .HasMaxLength(200);

	//	builder.Property(u => u.TargetApis)
	//		   .HasColumnName("TargetApis")
	//		   .HasMaxLength(1000);

	//	builder.Property(u => u.IsActive)
	//		   .HasColumnName("IsActive")
	//		   .IsRequired();

	//	builder.Ignore(u => u.Endpoints);
	//}

}