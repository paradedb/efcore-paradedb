using Autocomplete.Models;
using Microsoft.EntityFrameworkCore;

namespace Shared
{
    public class MockItemsDbContext<TContext, TItem> : DbContext
        where TContext : DbContext
        where TItem : MockItem
    {
        public DbSet<TItem> MockItems { get; set; }

        public MockItemsDbContext(DbContextOptions<TContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TItem>().Property(e => e.Id).ValueGeneratedNever();
        }
    }

    public class AutocompleteDbContext<TContext> : DbContext
        where TContext : DbContext
    {
        public DbSet<AutocompleteItem> AutocompleteItems { get; set; }

        public AutocompleteDbContext(DbContextOptions<TContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AutocompleteItem>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<AutocompleteItem>().Property(e => e.Category).HasMaxLength(100);
        }
    }
}

namespace Autocomplete.Data
{
    public class AppDbContext : Shared.AutocompleteDbContext<AppDbContext>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }
}

namespace FacetedSearch.Data
{
    public class AppDbContext : Shared.MockItemsDbContext<AppDbContext, Shared.MockItem>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }
}

namespace MoreLikeThis.Data
{
    public class AppDbContext : Shared.MockItemsDbContext<AppDbContext, Shared.MockItem>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }
}

namespace Quickstart.Data
{
    public class AppDbContext : Shared.MockItemsDbContext<AppDbContext, Shared.MockItem>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }
}

namespace Rag.Data
{
    public class AppDbContext : Shared.MockItemsDbContext<AppDbContext, Shared.MockItem>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
    }
}

namespace HybridRrf.Data
{
    public class AppDbContext : Shared.MockItemsDbContext<AppDbContext, MockItemWithEmbedding>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<MockItemWithEmbedding>()
                .Property(x => x.Embedding)
                .HasColumnType("vector(384)");
        }
    }
}
