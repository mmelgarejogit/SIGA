using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class TratamientoConfiguration : IEntityTypeConfiguration<Tratamiento>
{
    public void Configure(EntityTypeBuilder<Tratamiento> builder)
    {
        builder.ToTable("tratamientos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}
