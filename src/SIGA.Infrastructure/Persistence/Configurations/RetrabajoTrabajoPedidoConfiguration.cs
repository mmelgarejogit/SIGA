using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class RetrabajoTrabajoPedidoConfiguration : IEntityTypeConfiguration<RetrabajoTrabajoPedido>
{
    public void Configure(EntityTypeBuilder<RetrabajoTrabajoPedido> builder)
    {
        builder.ToTable("retrabajos_trabajo_pedido");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Motivo).HasConversion<int>().IsRequired();
        builder.Property(x => x.Responsable).HasConversion<int>().IsRequired();
        builder.Property(x => x.Observacion).HasMaxLength(1000);
        builder.Property(x => x.Fecha).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.TrabajoPedido)
            .WithMany(x => x.Retrabajos)
            .HasForeignKey(x => x.TrabajoPedidoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.RegistradoPor)
            .WithMany()
            .HasForeignKey(x => x.RegistradoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
