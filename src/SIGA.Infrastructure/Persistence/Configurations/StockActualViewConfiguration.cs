using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class StockActualViewConfiguration : IEntityTypeConfiguration<StockActualView>
{
    public void Configure(EntityTypeBuilder<StockActualView> builder)
    {
        builder.HasNoKey();
        builder.ToView("vw_stock_actual");

        builder.Property(v => v.ProductoId).HasColumnName("producto_id");
        builder.Property(v => v.SucursalId).HasColumnName("sucursal_id");
        builder.Property(v => v.StockActual).HasColumnName("stock_actual");
    }
}
