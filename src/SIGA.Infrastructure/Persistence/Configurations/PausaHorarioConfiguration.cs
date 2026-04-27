using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class PausaHorarioConfiguration : IEntityTypeConfiguration<PausaHorario>
{
    public void Configure(EntityTypeBuilder<PausaHorario> builder)
    {
        builder.ToTable("pausas_horario");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.HoraInicio).IsRequired().HasColumnType("time");
        builder.Property(x => x.HoraFin).IsRequired().HasColumnType("time");
        builder.Property(x => x.Descripcion).HasMaxLength(100);

        builder.HasOne(x => x.HorarioProfesional)
            .WithMany(x => x.Pausas)
            .HasForeignKey(x => x.HorarioProfesionalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
