using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace ContentAPI;

public class Content
{
    public int Id { get; init; }
    public Guid guid { get; init; } = Guid.NewGuid();
    public string text  { get; set; }
    public Content()
    {
    }

    public Content(int id, Guid guid, string text)
    {
        if (id < 0)
        {
            throw new ArgumentException("ID cannot be negative", nameof(id));
        }

        Id = id;
        this.guid = guid;
        this.text = text;
    }
}

public class DBContext : DbContext
{
    

    public DbSet<Content> Contents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=contents.db");
        }
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Content>().HasKey(e => e.Id);

        modelBuilder.Entity<Content>().HasIndex(sh => sh.guid).IsUnique();
    }
}