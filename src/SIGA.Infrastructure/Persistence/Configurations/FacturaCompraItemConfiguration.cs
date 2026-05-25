using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class FacturaCompraItemConfiguration : IEntityTypeConfiguration<FacturaCompraItem>
{
    public void Configure(EntityTypeBuilder<FacturaCompraItem> builder)
    {
        builder.ToTable("FacturaCompraItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Descripcion).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Cantidad).IsRequired();
        builder.Property(x => x.PrecioUnitario).IsRequired().HasColumnType("numeric(18,0)");
        builder.Property(x => x.TipoIva).HasConversion<int>().IsRequired();

        builder.Ignore(x => x.Total);

        builder.HasOne(x => x.FacturaCompra)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.FacturaCompraId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Producto)
            .WithMany()
            .HasForeignKey(x => x.ProductoId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
