using Domain.Entities.Notification;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.Notification;

public class NotificationHistoryConfiguration : IEntityTypeConfiguration<NotificationHistory>
{
	public void Configure(EntityTypeBuilder<NotificationHistory> builder)
	{
		builder.ToTable("Des2_NotificationHistory");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.Recipients)
			.IsRequired();

		builder.Property(e => e.Subject)
			.HasMaxLength(500)
			.IsRequired();

		builder.Property(e => e.Content)
			.IsRequired();

		builder.Property(e => e.SentTime)
			.IsRequired()
			.HasDefaultValueSql("GETUTCDATE()");

		builder.Property(e => e.IsSuccess)
			.IsRequired()
			.HasDefaultValue(false);

		// ErrorNotification ile ilişki
		builder.HasOne(e => e.ErrorNotification)
			.WithMany()
			.HasForeignKey(e => e.ErrorNotificationId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}