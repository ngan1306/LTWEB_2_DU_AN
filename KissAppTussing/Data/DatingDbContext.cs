using KissAppTussing.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KissAppTussing.Data;

internal sealed class DatingDbContext(DbContextOptions<DatingDbContext> options) : DbContext(options)
{
    private static readonly ValueConverter<DateTimeOffset, long> UtcDateTimeOffsetConverter = new(
        value => value.ToUnixTimeMilliseconds(),
        value => DateTimeOffset.FromUnixTimeMilliseconds(value));

    public DbSet<User> Users => Set<User>();
    public DbSet<Swipe> Swipes => Set<Swipe>();
    public DbSet<DatingMatch> Matches => Set<DatingMatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(user => user.NormalizedEmail)
            .IsUnique();
        modelBuilder.Entity<User>()
            .Property(user => user.CreatedAt)
            .HasConversion(UtcDateTimeOffsetConverter);

        modelBuilder.Entity<Swipe>()
            .HasIndex(swipe => new { swipe.FromUserId, swipe.ToUserId })
            .IsUnique();
        modelBuilder.Entity<Swipe>()
            .Property(swipe => swipe.CreatedAt)
            .HasConversion(UtcDateTimeOffsetConverter);

        modelBuilder.Entity<DatingMatch>()
            .HasIndex(match => new { match.FirstUserId, match.SecondUserId })
            .IsUnique();
        modelBuilder.Entity<DatingMatch>()
            .Property(match => match.MatchedAt)
            .HasConversion(UtcDateTimeOffsetConverter);

        modelBuilder.Entity<DatingMatch>()
            .HasOne(match => match.FirstUser)
            .WithMany()
            .HasForeignKey(match => match.FirstUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DatingMatch>()
            .HasOne(match => match.SecondUser)
            .WithMany()
            .HasForeignKey(match => match.SecondUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}