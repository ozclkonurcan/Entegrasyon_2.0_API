using Domain.Entities.WTPartModels.AlternateRemovedModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Configurations.WTPartEntityConfiguration;

public class WTPartAlternateLinkRemovedConfiguration : IEntityTypeConfiguration<WTPartAlternateLinkRemovedEntegration>
{
	public void Configure(EntityTypeBuilder<WTPartAlternateLinkRemovedEntegration> builder)
	{
		builder.ToTable("Des2_WTPart_AlternateLinkRemoved").HasKey(b => b.LogID);

		builder.Property(b => b.LogID).HasColumnName("LogID").IsRequired();
		builder.Property(b => b.EntegrasyonDurum).HasColumnName("EntegrasyonDurum");

		builder.Property(b => b.AnaParcaState).HasColumnName("AnaParcaState").IsRequired();
		builder.Property(b => b.AnaParcaPartID).HasColumnName("AnaParcaPartID");
		builder.Property(b => b.AnaParcaPartMasterID).HasColumnName("AnaParcaPartMasterID");
		builder.Property(b => b.AnaParcaName).HasColumnName("AnaParcaName").IsRequired();
		builder.Property(b => b.AnaParcaNumber).HasColumnName("AnaParcaNumber").IsRequired();
		builder.Property(b => b.AnaParcaVersion).HasColumnName("AnaParcaVersion").IsRequired();

		builder.Property(b => b.MuadilParcaState).HasColumnName("MuadilParcaState").IsRequired();
		builder.Property(b => b.MuadilParcaPartID).HasColumnName("MuadilParcaPartID");
		builder.Property(b => b.MuadilParcaMasterID).HasColumnName("MuadilParcaMasterID");
		builder.Property(b => b.MuadilParcaName).HasColumnName("MuadilParcaName").IsRequired();
		builder.Property(b => b.MuadilParcaNumber).HasColumnName("MuadilParcaNumber").IsRequired();
		builder.Property(b => b.MuadilParcaVersion).HasColumnName("MuadilParcaVersion").IsRequired();

		builder.Property(b => b.KulAd).HasColumnName("KulAd");
		builder.Property(b => b.LogDate).HasColumnName("LogDate");
		builder.Property(b => b.LogMesaj).HasColumnName("LogMesaj");
		//builder.Property(b => b.EntegrasyonTarihi).HasColumnName("EntegrasyonTarihi");
		//builder.Property(b => b.EntegrasyonHataMesaji).HasColumnName("EntegrasyonHataMesaji");

		// Ana parça ve muadil parça numaralarının birlikte benzersiz olmasını sağlayan bir indeks
		builder.HasIndex(
			indexExpression: b => new { b.AnaParcaNumber, b.MuadilParcaNumber },
			name: "UK_WTPartAlternateLinkRemoved_AnaParca_MuadilParca"
		).IsUnique();
	}
}