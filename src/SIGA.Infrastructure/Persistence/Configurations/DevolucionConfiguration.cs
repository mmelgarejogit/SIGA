using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class DevolucionConfiguration : IEntityTypeConfiguration<Devolucion>
{
    public void Configure(EntityTypeBuilder<Devolucion> builder)
    {
        builder.ToTable("devoluciones");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Tipo).HasConversion<int>().IsRequired();
        builder.Property(x => x.Estado).HasConversion<int>().IsRequired();
        builder.Property(x => x.Motivo).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.ObservacionesRevision).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.SolicitadoPor)
            .WithMany()
            .HasForeignKey(x => x.SolicitadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ConfirmadoPor)
            .WithMany()
            .HasForeignKey(x => x.ConfirmadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lineas)
            .WithOne(x => x.Devolucion)
            .HasForeignKey(x => x.DevolucionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
