using Domain.Entities.MailService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.MailService;
public class MailSettingsConfiguration : IEntityTypeConfiguration<MailSettings>
{
	public void Configure(EntityTypeBuilder<MailSettings> builder)
	{
		builder.ToTable("Des2_MailSettings");

		builder.HasKey(x => x.Id);

		// BaseEntities'den gelen alanlar
		builder.Property(x => x.CreatedDate)
			.IsRequired()
			.HasDefaultValueSql("GETDATE()");

		builder.Property(x => x.UpdatedDate)
			.IsRequired(false);

		builder.Property(x => x.DeletedDate)
			.IsRequired(false);

		// Kendi alanları
		builder.Property(x => x.Id)
			.ValueGeneratedOnAdd();

		builder.Property(x => x.SmtpServer)
			.IsRequired()
			.HasMaxLength(255);

		builder.Property(x => x.SmtpPort)
			.IsRequired()
			.HasDefaultValue(587);

		builder.Property(x => x.SmtpUsername)
			.IsRequired()
			.HasMaxLength(255);

		builder.Property(x => x.SmtpPassword)
			.IsRequired()
			.HasMaxLength(500);

		builder.Property(x => x.EnableSsl)
			.IsRequired()
			.HasDefaultValue(true);

		builder.Property(x => x.FromEmail)
			.IsRequired()
			.HasMaxLength(255);

		builder.Property(x => x.FromDisplayName)
			.HasMaxLength(255);

		builder.Property(x => x.SendOnError)
			.IsRequired()
			.HasDefaultValue(true);

		builder.Property(x => x.SendOnSuccess)
			.IsRequired()
			.HasDefaultValue(false);

		builder.Property(x => x.SendOnFinalFailure)
			.IsRequired()
			.HasDefaultValue(true);

		builder.Property(x => x.IsActive)
			.IsRequired()
			.HasDefaultValue(true);

		// Navigation
		builder.HasMany(x => x.MailRecipients)
			.WithOne(x => x.MailSettings)
			.HasForeignKey(x => x.MailSettingsId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}