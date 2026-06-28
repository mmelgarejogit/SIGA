using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class RecepcionMercaderiaConfiguration : IEntityTypeConfiguration<RecepcionMercaderia>
{
    public void Configure(EntityTypeBuilder<RecepcionMercaderia> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FechaRecepcion).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Observaciones).HasMaxLength(500);

        builder.HasOne(x => x.PedidoProveedor)
            .WithMany(p => p.Recepciones)
            .HasForeignKey(x => x.PedidoProveedorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FacturaCompra)
            .WithMany()
            .HasForeignKey(x => x.FacturaCompraId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Sucursal)
            .WithMany()
            .HasForeignKey(x => x.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
