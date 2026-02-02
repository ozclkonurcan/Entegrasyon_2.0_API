using Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.Notification;

public class EmailSettingsConfiguration : IEntityTypeConfiguration<EmailSettings>
{
	public void Configure(EntityTypeBuilder<EmailSettings> builder)
	{
		builder.ToTable("Des2_EmailSettings");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Host)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(e => e.Username)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(e => e.Password)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(e => e.FromEmail)
			.HasMaxLength(100)
			.IsRequired();

		builder.Property(e => e.IsActive)
			.HasDefaultValue(true)
			.IsRequired();

		builder.HasMany(e => e.Recipients)
			.WithOne(r => r.EmailSettings)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
