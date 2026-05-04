using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class HorarioProfesionalConfiguration : IEntityTypeConfiguration<HorarioProfesional>
{
    public void Configure(EntityTypeBuilder<HorarioProfesional> builder)
    {
        builder.ToTable("horarios_profesional");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DiaSemana).IsRequired().HasConversion<int>();
        builder.Property(x => x.HoraInicio).IsRequired().HasColumnType("time");
        builder.Property(x => x.HoraFin).IsRequired().HasColumnType("time");
        builder.Property(x => x.Activo).IsRequired();

        builder.HasIndex(x => new { x.ProfessionalId, x.DiaSemana }).IsUnique();

        builder.HasOne(x => x.Professional)
            .WithMany(x => x.Horarios)
            .HasForeignKey(x => x.ProfessionalId);
    }
}
