using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ProfesionalEspecialidadConfiguration : IEntityTypeConfiguration<ProfesionalEspecialidad>
{
    public void Configure(EntityTypeBuilder<ProfesionalEspecialidad> builder)
    {
        builder.ToTable("profesional_especialidades");
        builder.HasKey(x => new { x.ProfessionalId, x.EspecialidadId });

        builder.HasOne(x => x.Professional)
            .WithMany(x => x.Especialidades)
            .HasForeignKey(x => x.ProfessionalId);

        builder.HasOne(x => x.Especialidad)
            .WithMany(x => x.Profesionales)
            .HasForeignKey(x => x.EspecialidadId);
    }
}
