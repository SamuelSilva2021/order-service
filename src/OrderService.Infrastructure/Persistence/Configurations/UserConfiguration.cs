using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Users;

namespace OrderService.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);

        builder.Property(user => user.Username).HasMaxLength(64).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(user => user.CustomerId).HasMaxLength(64);

        builder.HasIndex(user => user.Username).IsUnique();

        builder.HasData(UserSeed.All());
    }
}
