using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ServicioTarifaConfiguration : IEntityTypeConfiguration<ServicioTarifa>
{
    public void Configure(EntityTypeBuilder<ServicioTarifa> builder)
    {
        builder.ToTable("servicio_tarifas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Precio).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.Servicio)
            .WithMany(s => s.Tarifas)
            .HasForeignKey(x => x.ServicioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Professional)
            .WithMany()
            .HasForeignKey(x => x.ProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Especialidad)
            .WithMany()
            .HasForeignKey(x => x.EspecialidadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ServicioId);
    }
}
