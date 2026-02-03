//using Domain.Entities.EPMModels.Equivalence;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Persistence.Configurations.EPMDocumentEntityConfiguration;

//// 1. ANA TABLO CONF
//public class EPMDocumentEquivalenceConfiguration : IEntityTypeConfiguration<EPMDocument_Equivalence>
//{
//	public void Configure(EntityTypeBuilder<EPMDocument_Equivalence> builder)
//	{
//		builder.ToTable("Des2_EPMDocument_Equivalence").HasKey(x => x.Ent_ID);
//		// Diğer property'ler isimleri aynı olduğu için otomatik eşleşir.
//		// LinkID Unique Index olabilir, performans için iyi olur:
//		// builder.HasIndex(x => x.LinkID); 
//	}
//}

//// 2. SENT CONF
//public class EPMDocumentEquivalenceSentConfiguration : IEntityTypeConfiguration<EPMDocument_Equivalence_Sent>
//{
//	public void Configure(EntityTypeBuilder<EPMDocument_Equivalence_Sent> builder)
//	{
//		builder.ToTable("Des2_EPMDocument_Equivalence_Sent").HasKey(x => x.Ent_ID);
//	}
//}

//// 3. ERROR CONF
//public class EPMDocumentEquivalenceErrorConfiguration : IEntityTypeConfiguration<EPMDocument_Equivalence_Error>
//{
//	public void Configure(EntityTypeBuilder<EPMDocument_Equivalence_Error> builder)
//	{
//		builder.ToTable("Des2_EPMDocument_Equivalence_Error").HasKey(x => x.Ent_ID);
//		builder.Property(b => b.RetryCount).HasDefaultValue(0);
//	}
//}

//// --- REMOVED KISMI ---

//public class EPMDocumentEquivalenceRemovedConfiguration : IEntityTypeConfiguration<EPMDocument_EquivalenceRemoved>
//{
//	public void Configure(EntityTypeBuilder<EPMDocument_EquivalenceRemoved> builder)
//	{
//		builder.ToTable("Des2_EPMDocument_EquivalenceRemoved").HasKey(x => x.Ent_ID);
//	}
//}

//public class EPMDocumentEquivalenceRemovedSentConfiguration : IEntityTypeConfiguration<EPMDocument_EquivalenceRemoved_Sent>
//{
//	public void Configure(EntityTypeBuilder<EPMDocument_EquivalenceRemoved_Sent> builder)
//	{
//		builder.ToTable("Des2_EPMDocument_EquivalenceRemoved_Sent").HasKey(x => x.Ent_ID);
//	}
//}

//public class EPMDocumentEquivalenceRemovedErrorConfiguration : IEntityTypeConfiguration<EPMDocument_EquivalenceRemoved_Error>
//{
//	public void Configure(EntityTypeBuilder<EPMDocument_EquivalenceRemoved_Error> builder)
//	{
//		builder.ToTable("Des2_EPMDocument_EquivalenceRemoved_Error").HasKey(x => x.Ent_ID);
//		builder.Property(b => b.RetryCount).HasDefaultValue(0);
//	}
//}