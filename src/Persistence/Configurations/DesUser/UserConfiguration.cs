using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Auth;
using Security.Hashing;

namespace Persistence.Configurations.DesUser;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("Des2_Users").HasKey(u => u.Id);

		builder.Property(u => u.Id).HasColumnName("Id").IsRequired();
		//builder.Property(u => u.UserId).HasColumnName("UserId");
		//builder.Property(u => u.TransferId).HasColumnName("TransferId");
		builder.Property(u => u.Email).HasColumnName("Email").IsRequired();
		builder.Property(u => u.FullName).HasColumnName("FullName");
		builder.Property(u => u.PasswordSalt).HasColumnName("PasswordSalt").IsRequired();
		builder.Property(u => u.PasswordHash).HasColumnName("PasswordHash").IsRequired();
		builder.Property(u => u.AuthenticatorType).HasColumnName("AuthenticatorType");
		builder.Property(u => u.CreatedDate).HasColumnName("CreatedDate");
		builder.Property(u => u.UpdatedDate).HasColumnName("UpdatedDate");
		builder.Property(u => u.DeletedDate).HasColumnName("DeletedDate");

		//builder.HasQueryFilter(u => !u.DeletedDate.HasValue);

		//builder.HasMany(u => u.UserOperationClaims);
		//builder.HasMany(u => u.RefreshTokens);
		//builder.HasMany(u => u.EmailAuthenticators);
		//builder.HasMany(u => u.OtpAuthenticators);

		//builder.HasData(_seeds);

		//builder.HasBaseType((string)null!);
	}

//	public static Guid AdminId { get; } = Guid.NewGuid();
//	private IEnumerable<User> _seeds
//	{
//		get
//		{
//			HashingHelper.CreatePasswordHash(
//				password: "Passw0rd!",
//				passwordHash: out byte[] passwordHash,
//				passwordSalt: out byte[] passwordSalt
//			);
//			User adminUser =
//				new()
//				{
//					Id = 1,
//					TransferId = AdminId,
//					Email = "narch@kodlama.io",
//					PasswordHash = passwordHash,
//					PasswordSalt = passwordSalt
//				};
//			yield return adminUser;
//		}
//	}
//
}