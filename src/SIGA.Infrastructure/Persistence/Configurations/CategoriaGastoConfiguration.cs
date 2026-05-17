using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class CategoriaGastoConfiguration : IEntityTypeConfiguration<CategoriaGasto>
{
    public void Configure(EntityTypeBuilder<CategoriaGasto> builder)
    {
        builder.ToTable("categorias_gasto");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Descripcion).HasMaxLength(500);
        builder.Property(x => x.Activo).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
