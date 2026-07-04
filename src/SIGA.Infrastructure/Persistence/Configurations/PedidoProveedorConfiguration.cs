using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class PedidoProveedorConfiguration : IEntityTypeConfiguration<PedidoProveedor>
{
    public void Configure(EntityTypeBuilder<PedidoProveedor> builder)
    {
        builder.HasOne(x => x.Sucursal)
            .WithMany()
            .HasForeignKey(x => x.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
