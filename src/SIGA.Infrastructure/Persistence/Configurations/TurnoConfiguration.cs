using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> builder)
    {
        builder.ToTable("turnos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FechaHora).IsRequired();
        builder.Property(x => x.Estado).HasConversion<int>().IsRequired();
        builder.Property(x => x.SolicitudCancelacion).IsRequired();
        builder.Property(x => x.Motivo).HasMaxLength(300);
        builder.Property(x => x.Notas).HasMaxLength(1000);

        builder.HasIndex(x => new { x.ProfessionalId, x.FechaHora }).IsUnique();

        builder.HasOne(x => x.Professional)
            .WithMany(p => p.Turnos)
            .HasForeignKey(x => x.ProfessionalId);

        builder.HasOne(x => x.Patient)
            .WithMany(p => p.Turnos)
            .HasForeignKey(x => x.PatientId);
    }
}
