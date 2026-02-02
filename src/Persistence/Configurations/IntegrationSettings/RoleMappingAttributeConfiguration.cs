using Domain.Entities.IntegrationSettings;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.IntegrationSettings;


	public class RoleMappingAttributeConfiguration : IEntityTypeConfiguration<RoleMappingAttribute>
	{
		public void Configure(EntityTypeBuilder<RoleMappingAttribute> builder)
		{
			// Tablo adı
			builder.ToTable("Des2_RolAyarlariAttributes");

			// Birincil anahtar ve alan adlandırmaları
			builder.HasKey(a => a.Id);

			builder.Property(a => a.Id)
				   .HasColumnName("AttributeMappingID")
				   .IsRequired();

			builder.Property(a => a.RoleMappingId)
				   .HasColumnName("RoleMappingID")
				   .IsRequired();

			builder.Property(a => a.AttributeName)
				   .HasColumnName("AttributeName")
				   .HasMaxLength(100)
				   .IsRequired();

			builder.Property(a => a.IsSelected)
				   .HasColumnName("IsSelected")
				   .IsRequired();

			// İlişkilendirme: Eğer RoleMapping silinirse, cascade delete ile bağlı attribute kayıtları da silinecek
			builder.HasOne(a => a.RoleMapping)
				   .WithMany(r => r.WindchillAttributes)
				   .HasForeignKey(a => a.RoleMappingId)
				   .OnDelete(DeleteBehavior.Cascade);
		}
	}
