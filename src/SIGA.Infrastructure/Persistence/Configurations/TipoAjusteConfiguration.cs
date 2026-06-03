using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class TipoAjusteConfiguration : IEntityTypeConfiguration<TipoAjuste>
{
    public void Configure(EntityTypeBuilder<TipoAjuste> b)
    {
        b.ToTable("tipos_ajuste");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
        b.Property(x => x.Impacto).HasConversion<string>();
    }
}
