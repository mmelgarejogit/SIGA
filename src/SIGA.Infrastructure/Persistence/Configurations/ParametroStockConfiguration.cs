using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ParametroStockConfiguration : IEntityTypeConfiguration<ParametroStock>
{
    public void Configure(EntityTypeBuilder<ParametroStock> b)
    {
        b.ToTable("parametros_stock");
        b.HasKey(x => new { x.ProductoVarianteId, x.SucursalId });

        b.HasOne(x => x.ProductoVariante)
            .WithMany(p => p.ParametrosStock)
            .HasForeignKey(x => x.ProductoVarianteId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Sucursal)
            .WithMany(s => s.ParametrosStock)
            .HasForeignKey(x => x.SucursalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
