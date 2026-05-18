using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class FacturaCompraConfiguration : IEntityTypeConfiguration<FacturaCompra>
{
    public void Configure(EntityTypeBuilder<FacturaCompra> builder)
    {
        builder.Property(x => x.NroFactura).HasMaxLength(15);

        builder.Property(x => x.MontoExento).IsRequired().HasColumnType("numeric(18,0)");
        builder.Property(x => x.MontoGravado5).IsRequired().HasColumnType("numeric(18,0)");
        builder.Property(x => x.MontoGravado10).IsRequired().HasColumnType("numeric(18,0)");
        builder.Property(x => x.CondicionVenta).HasConversion<int>().IsRequired();

        // Calculados en dominio — EF no persiste columnas separadas
        builder.Ignore(x => x.Iva5);
        builder.Ignore(x => x.Iva10);
        builder.Ignore(x => x.MontoTotal);

        builder.HasOne(x => x.Proveedor)
            .WithMany()
            .HasForeignKey(x => x.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PedidoProveedor)
            .WithMany()
            .HasForeignKey(x => x.PedidoProveedorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
