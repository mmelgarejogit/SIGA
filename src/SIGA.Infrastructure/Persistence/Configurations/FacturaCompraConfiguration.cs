using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class FacturaCompraConfiguration : IEntityTypeConfiguration<FacturaCompra>
{
    public void Configure(EntityTypeBuilder<FacturaCompra> builder)
    {
        builder.Property(x => x.NroFactura).HasMaxLength(100);

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
