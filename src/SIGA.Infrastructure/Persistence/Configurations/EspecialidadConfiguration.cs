using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class EspecialidadConfiguration : IEntityTypeConfiguration<Especialidad>
{
    public void Configure(EntityTypeBuilder<Especialidad> builder)
    {
        builder.ToTable("especialidades");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(150);
        builder.HasIndex(x => x.Nombre).IsUnique();

        builder.Property(x => x.Descripcion).HasMaxLength(500);
    }
}
