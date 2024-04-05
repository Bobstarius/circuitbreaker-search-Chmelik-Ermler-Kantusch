using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace ContentAPI;

public class SearchHistory
{
    public int Id { get; init; }
    public Guid guid { get; init; } = Guid.NewGuid();
    public string searchUsername  { get; set; }
    public string searchContent { get; set; }
    public SearchHistory()
    {
    }

    public SearchHistory(int id, Guid guid, string searchUsername, string searchContent)
    {
        if (id < 0)
        {
            throw new ArgumentException("ID cannot be negative", nameof(id));
        }

        Id = id;
        this.guid = guid;
        this.searchUsername = searchUsername;
        this.searchContent = searchContent;
    }
}

public class DBContext : DbContext
{
    public DbSet<SearchHistory> SearchHistories { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=searchhistories.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SearchHistory>().HasKey(sh => sh.Id);

        // Zusätzliche Konfigurationen für die SearchHistory-Tabelle können hier hinzugefügt werden
        // Zum Beispiel, wenn Sie eine einzigartige Einschränkung für 'guid' wollen:
        modelBuilder.Entity<SearchHistory>().HasIndex(sh => sh.guid).IsUnique();
        
        // Andere Konfigurationen nach Bedarf
    }
}