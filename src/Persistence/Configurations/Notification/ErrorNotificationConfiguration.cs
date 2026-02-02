using Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.Notification;

public class ErrorNotificationConfiguration : IEntityTypeConfiguration<ErrorNotification>
{
	public void Configure(EntityTypeBuilder<ErrorNotification> builder)
	{
		builder.ToTable("Des2_ErrorNotification");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.ErrorKey)
			.HasMaxLength(255)
			.IsRequired();

		builder.Property(e => e.ErrorType)
			.HasMaxLength(255)
			.IsRequired();

		builder.Property(e => e.ErrorMessage)
			.IsRequired();

		builder.Property(e => e.OperationType)
			.HasMaxLength(255)
			.IsRequired();

		builder.Property(e => e.ErrorCount)
			.HasDefaultValue(1)
			.IsRequired();

		builder.Property(e => e.FirstOccurrence)
			.IsRequired();

		builder.Property(e => e.LastOccurrence)
			.IsRequired();

		builder.Property(e => e.LastNotificationTime);

		builder.Property(e => e.IsActive)
			.HasDefaultValue(true)
			.IsRequired();

		builder.Property(e => e.CreatedDate)
			.IsRequired()
			.HasDefaultValueSql("GETUTCDATE()");

		builder.Property(e => e.UpdatedDate)
			.IsRequired()
			.HasDefaultValueSql("GETUTCDATE()");

		// NotificationHistory ile ilişki
		builder.HasMany<NotificationHistory>()
			.WithOne(n => n.ErrorNotification)
			.HasForeignKey(n => n.ErrorNotificationId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}