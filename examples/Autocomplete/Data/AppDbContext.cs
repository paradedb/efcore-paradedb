using Autocomplete.Models;
using Microsoft.EntityFrameworkCore;

namespace Autocomplete.Data;

public class AppDbContext : DbContext
{
    public DbSet<AutocompleteItem> AutocompleteItems { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AutocompleteItem>().Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<AutocompleteItem>().Property(e => e.Category).HasMaxLength(100);
    }
}
