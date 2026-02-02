using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.IntegrationSettings;

namespace Persistence.Configurations.IntegrationSettings;

public class IntegrationModuleSettingsConfiguration : IEntityTypeConfiguration<IntegrationModuleSettings>
{
	public void Configure(EntityTypeBuilder<IntegrationModuleSettings> builder)
	{
		// Tablo adını ve şemayı ayarlayın (örneğin, "Des2_Module_Settings" tablosu, "PLM1" şeması)
		builder.ToTable("Des2_Module_Settings");

		// Primary key tanımı
		builder.HasKey(u => u.Id);

		// Id sütunu
		builder.Property(u => u.Id)
			   .HasColumnName("ModuleSettingsID")
			   .IsRequired();

		// SettingsName sütunu (örneğin, 100 karakter ile sınırlandırılmış ve zorunlu)
		builder.Property(u => u.SettingsName)
			   .HasColumnName("SettingsName")
			   .HasMaxLength(100)
			   .IsRequired();

		// SettingsValue sütunu (örneğin, TINYINT olarak saklanacak, zorunlu)
		builder.Property(u => u.SettingsValue)
			   .HasColumnName("SettingsValue")
			   .IsRequired();
	}
}