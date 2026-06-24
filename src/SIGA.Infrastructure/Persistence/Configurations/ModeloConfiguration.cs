using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ModeloConfiguration : IEntityTypeConfiguration<Modelo>
{
    public void Configure(EntityTypeBuilder<Modelo> builder)
    {
        builder.ToTable("modelos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Marca)
            .WithMany(m => m.Modelos)
            .HasForeignKey(x => x.MarcaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.MarcaId, x.Nombre }).IsUnique();
    }
}
