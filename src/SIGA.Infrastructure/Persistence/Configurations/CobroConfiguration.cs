using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class CobroConfiguration : IEntityTypeConfiguration<Cobro>
{
    public void Configure(EntityTypeBuilder<Cobro> builder)
    {
        builder.ToTable("cobros");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Monto).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(x => x.MetodoPago).HasConversion<int>().IsRequired();
        builder.Property(x => x.FechaCobro).IsRequired();
        builder.Property(x => x.Referencia).HasMaxLength(200);
        builder.Property(x => x.Anulado).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
