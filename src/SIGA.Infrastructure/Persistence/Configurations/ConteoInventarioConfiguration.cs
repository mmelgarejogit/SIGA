using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ConteoInventarioConfiguration : IEntityTypeConfiguration<ConteoInventario>
{
    public void Configure(EntityTypeBuilder<ConteoInventario> builder)
    {
        builder.HasOne(c => c.Sucursal)
            .WithMany()
            .HasForeignKey(c => c.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
