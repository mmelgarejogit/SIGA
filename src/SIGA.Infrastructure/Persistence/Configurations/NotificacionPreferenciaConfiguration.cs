using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class NotificacionPreferenciaConfiguration : IEntityTypeConfiguration<NotificacionPreferencia>
{
    public void Configure(EntityTypeBuilder<NotificacionPreferencia> builder)
    {
        builder.ToTable("notificaciones_preferencias");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Person)
            .WithMany()
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PersonId).IsUnique();
    }
}
