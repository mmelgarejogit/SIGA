using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class RecepcionMercaderiaItemConfiguration : IEntityTypeConfiguration<RecepcionMercaderiaItem>
{
    public void Configure(EntityTypeBuilder<RecepcionMercaderiaItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Cantidad).IsRequired();
        builder.Property(x => x.Lote).HasMaxLength(80);
        builder.Property(x => x.Observaciones).HasMaxLength(500);

        builder.HasOne(x => x.Recepcion)
            .WithMany(r => r.Items)
            .HasForeignKey(x => x.RecepcionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PedidoItem)
            .WithMany()
            .HasForeignKey(x => x.PedidoItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
