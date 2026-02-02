using Domain.Entities.MailService;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.MailService;
public class MailRecipientConfiguration : IEntityTypeConfiguration<MailRecipient>
{
	public void Configure(EntityTypeBuilder<MailRecipient> builder)
	{
		builder.ToTable("Des2_MailRecipients");

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

		builder.Property(x => x.MailSettingsId)
			.IsRequired();

		builder.Property(x => x.EmailAddress)
			.IsRequired()
			.HasMaxLength(255);

		builder.Property(x => x.DisplayName)
			.HasMaxLength(255);

		builder.Property(x => x.IsActive)
			.IsRequired()
			.HasDefaultValue(true);

		// Unique constraint
		builder.HasIndex(x => new { x.MailSettingsId, x.EmailAddress })
			.IsUnique()
			.HasDatabaseName("UK_Des2_MailRecipients_Email_Settings");

		// Foreign Key
		builder.HasOne(x => x.MailSettings)
			.WithMany(x => x.MailRecipients)
			.HasForeignKey(x => x.MailSettingsId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}