using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("ventas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NumeroComprobante).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Estado).HasConversion<int>().IsRequired();
        builder.Property(x => x.CondicionVenta).HasConversion<int>().IsRequired();
        builder.Property(x => x.FechaVenta).IsRequired();
        builder.Property(x => x.FechaVencimiento);
        builder.Property(x => x.Observaciones).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.Ignore(x => x.MontoExento);
        builder.Ignore(x => x.MontoGravado5);
        builder.Ignore(x => x.MontoGravado10);
        builder.Ignore(x => x.Total);
        builder.Ignore(x => x.TotalCobrado);
        builder.Ignore(x => x.SaldoPendiente);

        builder.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Receta).WithMany().HasForeignKey(x => x.RecetaId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(x => x.Lineas).WithOne(x => x.Venta).HasForeignKey(x => x.VentaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Cobros).WithOne(x => x.Venta).HasForeignKey(x => x.VentaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Factura).WithOne(x => x.Venta).HasForeignKey<FacturaVenta>(x => x.VentaId).OnDelete(DeleteBehavior.Cascade);
    }
}
