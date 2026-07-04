using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class TransferenciaStockConfiguration : IEntityTypeConfiguration<TransferenciaStock>
{
    public void Configure(EntityTypeBuilder<TransferenciaStock> builder)
    {
        builder.ToTable("transferencias_stock");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Estado).IsRequired().HasMaxLength(20);
        builder.Property(x => x.CreadoPorId).HasMaxLength(50);
        builder.Property(x => x.CreadoPorNombre).HasMaxLength(200);
        builder.Property(x => x.RecibidoPorNombre).HasMaxLength(200);
        builder.Property(x => x.Observaciones).HasMaxLength(500);

        builder.HasOne(x => x.SucursalOrigen)
            .WithMany()
            .HasForeignKey(x => x.SucursalOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SucursalDestino)
            .WithMany()
            .HasForeignKey(x => x.SucursalDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(i => i.TransferenciaStock)
            .HasForeignKey(i => i.TransferenciaStockId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TransferenciaStockItemConfiguration : IEntityTypeConfiguration<TransferenciaStockItem>
{
    public void Configure(EntityTypeBuilder<TransferenciaStockItem> builder)
    {
        builder.ToTable("transferencias_stock_items");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Producto)
            .WithMany()
            .HasForeignKey(x => x.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
