using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class FacturaVentaConfiguration : IEntityTypeConfiguration<FacturaVenta>
{
    public void Configure(EntityTypeBuilder<FacturaVenta> builder)
    {
        builder.ToTable("facturas_venta");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NumeroFactura).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Timbrado).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Establecimiento).IsRequired().HasMaxLength(20);
        builder.Property(x => x.MontoExento).IsRequired().HasColumnType("numeric(18,0)");
        builder.Property(x => x.MontoGravado5).IsRequired().HasColumnType("numeric(18,0)");
        builder.Property(x => x.MontoGravado10).IsRequired().HasColumnType("numeric(18,0)");
        builder.Property(x => x.FechaEmision).IsRequired();
        builder.Property(x => x.Observaciones).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Ignore(x => x.Iva5);
        builder.Ignore(x => x.Iva10);
        builder.Ignore(x => x.Total);
    }
}
