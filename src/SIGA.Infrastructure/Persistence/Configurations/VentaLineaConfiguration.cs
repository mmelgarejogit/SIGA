using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class VentaLineaConfiguration : IEntityTypeConfiguration<VentaLinea>
{
    public void Configure(EntityTypeBuilder<VentaLinea> builder)
    {
        builder.ToTable("venta_lineas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Tipo).HasConversion<int>().IsRequired();
        builder.Property(x => x.Descripcion).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Cantidad).IsRequired();
        builder.Property(x => x.PrecioUnitario).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(x => x.Descuento).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(x => x.CategoriaFiscal).HasConversion<int>().IsRequired();

        builder.Ignore(x => x.Subtotal);

        builder.HasOne(x => x.ProductoVariante).WithMany(v => v.VentaLineas).HasForeignKey(x => x.ProductoVarianteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Servicio).WithMany().HasForeignKey(x => x.ServicioId).OnDelete(DeleteBehavior.Restrict);
    }
}
