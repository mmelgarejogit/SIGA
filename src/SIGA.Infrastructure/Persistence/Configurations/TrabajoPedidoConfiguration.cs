using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class TrabajoPedidoConfiguration : IEntityTypeConfiguration<TrabajoPedido>
{
    public void Configure(EntityTypeBuilder<TrabajoPedido> builder)
    {
        builder.ToTable("trabajos_pedido");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ArmazonDelCliente).HasDefaultValue(false);

        builder.HasOne(x => x.TipoLente)
            .WithMany(x => x.TrabajosPedido)
            .HasForeignKey(x => x.TipoLenteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Tratamientos)
            .WithMany(x => x.TrabajosPedido)
            .UsingEntity(j => j.ToTable("trabajos_pedido_tratamientos"));
        builder.Property(x => x.Estado).HasConversion<int>().IsRequired();
        builder.Property(x => x.MedioEnvio).HasConversion<int>();
        builder.Property(x => x.Observacion).HasMaxLength(1000);
        builder.Property(x => x.ObservacionAprobacion).HasMaxLength(500);
        builder.Property(x => x.RetiradoPor).HasMaxLength(200);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.VentaId).IsUnique();

        builder.HasOne(x => x.AprobadoPor)
            .WithMany()
            .HasForeignKey(x => x.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EntregadoPor)
            .WithMany()
            .HasForeignKey(x => x.EntregadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Factura)
            .WithOne(x => x.TrabajoPedido)
            .HasForeignKey<FacturaLaboratorio>(x => x.TrabajoPedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Receta)
            .WithMany()
            .HasForeignKey(x => x.RecetaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.ArmazonProducto)
            .WithMany()
            .HasForeignKey(x => x.ArmazonProductoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.LaboratorioProveedor)
            .WithMany()
            .HasForeignKey(x => x.LaboratorioProveedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
