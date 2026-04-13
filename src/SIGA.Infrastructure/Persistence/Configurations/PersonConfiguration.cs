using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("persons");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DNI).IsRequired().HasMaxLength(20);
        builder.HasIndex(x => x.DNI).IsUnique();

        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.BirthDate).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);

        builder.Property(x => x.Email).IsRequired().HasMaxLength(320);
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
