using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class GastoGeneralConfiguration : IEntityTypeConfiguration<GastoGeneral>
{
    public void Configure(EntityTypeBuilder<GastoGeneral> builder)
    {
        builder.HasOne(x => x.CategoriaGasto)
            .WithMany(c => c.Gastos)
            .HasForeignKey(x => x.CategoriaGastoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
