using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
{
    public void Configure(EntityTypeBuilder<Professional> builder)
    {
        builder.ToTable("professionals");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Specialty).IsRequired().HasMaxLength(150);
        builder.Property(x => x.LicenseNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.LicenseNumber).IsUnique();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.User)
            .WithOne(x => x.Professional)
            .HasForeignKey<Professional>(x => x.UserId);
    }
}
