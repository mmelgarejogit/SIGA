using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class EstadoConfigConfiguration : IEntityTypeConfiguration<EstadoConfig>
{
    public void Configure(EntityTypeBuilder<EstadoConfig> builder)
    {
        builder.ToTable("estados_config");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Entidad).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Color).IsRequired().HasMaxLength(20);
        builder.Property(x => x.CodigoInterno).HasMaxLength(50);
        builder.Property(x => x.EsProtegido).IsRequired();
        builder.Property(x => x.Orden).IsRequired();

        builder.HasIndex(x => new { x.Entidad, x.Nombre }).IsUnique();
    }
}
