using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ConsultaClinicaConfiguration : IEntityTypeConfiguration<ConsultaClinica>
{
    public void Configure(EntityTypeBuilder<ConsultaClinica> builder)
    {
        builder.ToTable("consultas_clinicas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Motivo).IsRequired().HasMaxLength(500);
        builder.Property(x => x.DiagnosticoPrincipal).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Anamnesis).HasMaxLength(2000);
        builder.Property(x => x.ExamenFisico).HasMaxLength(2000);
        builder.Property(x => x.DiagnosticoSecundario).HasMaxLength(500);
        builder.Property(x => x.PlanTratamiento).HasMaxLength(2000);
        builder.Property(x => x.Observaciones).HasMaxLength(2000);
        builder.Property(x => x.FechaConsulta).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Professional)
            .WithMany()
            .HasForeignKey(x => x.ProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
