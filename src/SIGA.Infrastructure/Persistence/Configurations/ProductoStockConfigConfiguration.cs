using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ProductoStockConfigConfiguration : IEntityTypeConfiguration<ProductoStockConfig>
{
    public void Configure(EntityTypeBuilder<ProductoStockConfig> builder)
    {
        builder.ToTable("productos_stock_config");
        builder.HasKey(c => c.ProductoId);

        builder.Property(c => c.ProductoId).HasColumnName("producto_id");
        builder.Property(c => c.StockMinimo).HasColumnName("stock_minimo").IsRequired();
        builder.Property(c => c.StockMaximo).HasColumnName("stock_maximo");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(c => c.Producto)
            .WithOne(p => p.StockConfig)
            .HasForeignKey<ProductoStockConfig>(c => c.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
