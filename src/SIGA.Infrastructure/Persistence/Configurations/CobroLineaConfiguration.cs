using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class CobroLineaConfiguration : IEntityTypeConfiguration<CobroLinea>
{
    public void Configure(EntityTypeBuilder<CobroLinea> builder)
    {
        builder.ToTable("cobro_lineas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MetodoPago).HasConversion<int>().IsRequired();
        builder.Property(x => x.Monto).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(x => x.Referencia).HasMaxLength(200);
    }
}
