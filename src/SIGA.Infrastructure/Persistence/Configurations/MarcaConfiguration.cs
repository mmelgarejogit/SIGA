using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class MarcaConfiguration : IEntityTypeConfiguration<Marca>
{
    public void Configure(EntityTypeBuilder<Marca> builder)
    {
        builder.ToTable("marcas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}
