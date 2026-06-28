using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class StockLoteConfiguration : IEntityTypeConfiguration<StockLote>
{
    public void Configure(EntityTypeBuilder<StockLote> builder)
    {
        builder.HasOne(l => l.Sucursal)
            .WithMany()
            .HasForeignKey(l => l.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
