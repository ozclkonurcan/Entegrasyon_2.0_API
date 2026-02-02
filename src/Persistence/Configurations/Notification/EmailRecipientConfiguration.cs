using Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.Notification;

public class EmailRecipientConfiguration : IEntityTypeConfiguration<EmailRecipient>
{
	public void Configure(EntityTypeBuilder<EmailRecipient> builder)
	{
		builder.ToTable("Des2_EmailRecipients");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.IsActive)
			.HasDefaultValue(true)
			.IsRequired();

		builder.HasOne(e => e.User)
			.WithMany()
			.HasForeignKey(e => e.UserId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
