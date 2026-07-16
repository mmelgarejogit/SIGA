using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class CobroConfiguration : IEntityTypeConfiguration<Cobro>
{
    public void Configure(EntityTypeBuilder<Cobro> builder)
    {
        builder.ToTable("cobros");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Tipo).HasConversion<int>().IsRequired();
        builder.Property(x => x.MontoTotal).IsRequired().HasColumnType("numeric(18,0)");
        builder.Property(x => x.Fecha).IsRequired();
        builder.Property(x => x.Anulado).IsRequired();
        builder.Property(x => x.RegistradoPorId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.RegistradoPor)
            .WithMany()
            .HasForeignKey(x => x.RegistradoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lineas)
            .WithOne(x => x.Cobro)
            .HasForeignKey(x => x.CobroId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
