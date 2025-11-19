using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

public class QuixoDbContext : DbContext
{
    public QuixoDbContext(DbContextOptions<QuixoDbContext> options) : base(options)
    {
    }
    
    public DbSet<Game> Games { get; set; }
    public DbSet<Move> Moves { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Game>()
            .HasMany(g => g.Moves)
            .WithOne(m => m.Game)
            .HasForeignKey(m => m.GameId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Game>()
            .Property(g => g.Mode)
            .HasConversion<int>();
        
        modelBuilder.Entity<Game>()
            .Property(g => g.Status)
            .HasConversion<int>();
        
        modelBuilder.Entity<Move>()
            .Property(m => m.Symbol)
            .HasConversion<int>();
        
        modelBuilder.Entity<Move>()
            .Property(m => m.PointDirection)
            .HasConversion<int?>();
    }
}

