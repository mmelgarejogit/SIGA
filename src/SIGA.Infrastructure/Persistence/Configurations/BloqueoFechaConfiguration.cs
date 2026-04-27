using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class BloqueoFechaConfiguration : IEntityTypeConfiguration<BloqueoFecha>
{
    public void Configure(EntityTypeBuilder<BloqueoFecha> builder)
    {
        builder.ToTable("bloqueos_fecha");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Fecha).IsRequired().HasColumnType("date");
        builder.Property(x => x.Motivo).HasMaxLength(300);

        builder.HasIndex(x => new { x.ProfessionalId, x.Fecha }).IsUnique();

        builder.HasOne(x => x.Professional)
            .WithMany(x => x.Bloqueos)
            .HasForeignKey(x => x.ProfessionalId);
    }
}
