using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ProductoVarianteConfiguration : IEntityTypeConfiguration<ProductoVariante>
{
    public void Configure(EntityTypeBuilder<ProductoVariante> b)
    {
        b.ToTable("producto_variantes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Sku).HasMaxLength(100);
        b.Property(x => x.Color).HasMaxLength(60);
        b.Property(x => x.Talle).HasMaxLength(30);
        b.Property(x => x.PrecioCosto).HasPrecision(18, 2);
        b.Property(x => x.PrecioVenta).HasPrecision(18, 2);
        b.Property(x => x.ImagenUrl).HasMaxLength(500);

        b.HasOne(x => x.Producto)
            .WithMany(p => p.Variantes)
            .HasForeignKey(x => x.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
