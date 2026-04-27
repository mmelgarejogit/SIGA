using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class RecetaConfiguration : IEntityTypeConfiguration<Receta>
{
    public void Configure(EntityTypeBuilder<Receta> builder)
    {
        builder.ToTable("recetas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OdEsferico).HasPrecision(5, 2);
        builder.Property(x => x.OdCilindro).HasPrecision(5, 2);
        builder.Property(x => x.OdAdicion).HasPrecision(5, 2);
        builder.Property(x => x.OiEsferico).HasPrecision(5, 2);
        builder.Property(x => x.OiCilindro).HasPrecision(5, 2);
        builder.Property(x => x.OiAdicion).HasPrecision(5, 2);
        builder.Property(x => x.DistanciaInterpupilar).HasPrecision(5, 2);
        builder.Property(x => x.AvSinCorreccion).HasMaxLength(20);
        builder.Property(x => x.AvConCorreccion).HasMaxLength(20);
        builder.Property(x => x.Observaciones).HasMaxLength(1000);

        builder.HasOne(x => x.ConsultaClinica)
            .WithOne(x => x.Receta)
            .HasForeignKey<Receta>(x => x.ConsultaClinicaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ConsultaClinicaId).IsUnique();
    }
}
