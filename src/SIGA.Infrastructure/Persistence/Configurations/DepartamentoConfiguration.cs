using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class DepartamentoConfiguration : IEntityTypeConfiguration<Departamento>
{
    public void Configure(EntityTypeBuilder<Departamento> builder)
    {
        builder.ToTable("departamentos");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Nombre).IsRequired().HasMaxLength(100);
        builder.HasIndex(d => d.Nombre).IsUnique();
        builder.Property(d => d.IsActive).HasDefaultValue(true);
        builder.Property(d => d.CreatedAt).HasDefaultValueSql("now()");
    }
}
