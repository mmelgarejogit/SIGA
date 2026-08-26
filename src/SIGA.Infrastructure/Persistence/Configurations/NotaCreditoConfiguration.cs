using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class NotaCreditoConfiguration : IEntityTypeConfiguration<NotaCredito>
{
    public void Configure(EntityTypeBuilder<NotaCredito> builder)
    {
        builder.ToTable("notas_credito");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NumeroNotaCredito).IsRequired().HasMaxLength(50);
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

        // 1:1 con Devolucion
        builder.HasOne(x => x.Devolucion).WithOne(d => d.NotaCredito)
            .HasForeignKey<NotaCredito>(x => x.DevolucionId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Venta).WithMany()
            .HasForeignKey(x => x.VentaId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FacturaVenta).WithMany()
            .HasForeignKey(x => x.FacturaVentaId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EmitidoPor).WithMany()
            .HasForeignKey(x => x.EmitidoPorId).OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TimbradoConfig).WithMany()
            .HasForeignKey(x => x.TimbradoId).OnDelete(DeleteBehavior.Restrict);
    }
}
