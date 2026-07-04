using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class MovimientoStockConfiguration : IEntityTypeConfiguration<MovimientoStock>
{
    public void Configure(EntityTypeBuilder<MovimientoStock> builder)
    {
        builder.HasOne(m => m.Sucursal)
            .WithMany()
            .HasForeignKey(m => m.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
