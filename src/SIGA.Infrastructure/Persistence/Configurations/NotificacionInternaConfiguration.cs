using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class NotificacionInternaConfiguration : IEntityTypeConfiguration<NotificacionInterna>
{
    public void Configure(EntityTypeBuilder<NotificacionInterna> builder)
    {
        builder.ToTable("notificaciones_internas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Tipo).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Mensaje).IsRequired().HasMaxLength(500);
        builder.Property(x => x.EntidadOrigenTipo).HasMaxLength(50);

        builder.HasOne(x => x.DestinatarioUsuario)
            .WithMany()
            .HasForeignKey(x => x.DestinatarioUsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.DestinatarioSucursal)
            .WithMany()
            .HasForeignKey(x => x.DestinatarioSucursalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.DestinatarioUsuarioId);
        builder.HasIndex(x => new { x.DestinatarioSucursalId, x.Leido });
    }
}
