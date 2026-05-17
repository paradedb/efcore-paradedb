using Microsoft.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.IntegrationTests.Persistence.Entities;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace ParadeDB.EntityFrameworkCore.IntegrationTests.Persistence;

public sealed class DbFixture : IAsyncInitializer, IAsyncDisposable
{
    private PostgreSqlContainer? _container;

    private DbContextOptions<TestDbContext> _options = null!;

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder("postgres:18")
            .WithImage("paradedb/paradedb:latest")
            .WithDatabase("pg_search_test")
            .WithUsername("test")
            .WithPassword("Pass!w0rd1")
            .Build();

        await _container.StartAsync();

        _options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(_container.GetConnectionString(), o => o.UseParadeDb())
            .UseSnakeCaseNamingConvention()
            .Options;

        await using var context = new TestDbContext(_options);
        await context.Database.MigrateAsync();
        await SeedDefaultDataAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    public TestDbContext CreateContext() => new(_options);

    private async Task SeedDefaultDataAsync()
    {
        await using var context = CreateContext();

        var products = new[]
        {
            new Product
            {
                Id = 1,
                Name = "UltraComfort Memory Foam Running Shoes",
                Description = """
                    Experience unparalleled comfort with the UltraComfort Memory Foam Running Shoes.
                    Designed for athletes and casual joggers alike, these shoes feature a breathable mesh upper,
                    a cushioned memory foam insole that molds to your foot's unique shape, and a durable rubber outsole.
                    Whether you're hitting the pavement or enjoying a leisurely walk, these shoes provide the support
                    and comfort you need.
                    """,
            },
            new Product
            {
                Id = 2,
                Name = "ProStride Jogging Sneakers",
                Description = """
                    Elevate your running experience with the ProStride Jogging Sneakers.
                    Crafted with a lightweight, breathable mesh upper, these sneakers offer optimal ventilation
                    to keep your feet cool. The responsive cushioning system absorbs impact, reducing strain on your joints,
                    while the slip-resistant rubber outsole ensures stability on various surfaces.
                    Perfect for both seasoned runners and beginners.
                    """,
            },
            new Product
            {
                Id = 3,
                Name = "NoiseCancel Wireless Headphones",
                Description = """
                    Immerse yourself in high-quality sound with the NoiseCancel Wireless Headphones.
                    Featuring advanced noise-cancelling technology, these headphones block out ambient noise,
                    allowing you to focus on your music or calls. The ergonomic design ensures a comfortable fit
                    for extended wear, and the long-lasting battery provides hours of uninterrupted listening.
                    Ideal for travel, work, or leisure.
                    """,
            },
            new Product
            {
                Id = 4,
                Name = "StudioSound Over-Ear Headphones",
                Description = """
                    Discover exceptional audio clarity with the StudioSound Over-Ear Headphones.
                    Equipped with premium drivers, these headphones deliver rich bass and crisp highs,
                    providing an immersive listening experience. The plush ear cups and adjustable headband
                    offer a personalized fit, while the foldable design makes them easy to store and transport.
                    Suitable for audiophiles and casual listeners alike.
                    """,
            },
            new Product
            {
                Id = 5,
                Name = "EcoBrew French Press Coffee Maker",
                Description = """
                    Brew your favorite coffee with the EcoBrew French Press Coffee Maker.
                    Made from high-quality borosilicate glass and stainless steel, this French press ensures durability
                    and heat retention. The fine mesh filter allows essential oils and fine particles to pass through,
                    delivering a rich and full-bodied cup of coffee. Its sleek design and easy-to-use mechanism
                    make it a must-have for coffee enthusiasts.
                    """,
            },
        };

        var items = new[]
        {
            new Item
            {
                Id = 1,
                Name = "Vintage Leather Journal",
                Description = """
                    A vintage leather journal featuring hand-stitched binding, aged parchment pages, and a brass clasp closure.
                    Each page has a subtle texture that makes writing a tactile pleasure, and the cover develops a rich patina over time.
                    Perfect for travelers, writers, and anyone who appreciates the art of analog note-taking.
                    """,
            },
            new Item
            {
                Id = 2,
                Name = "Bamboo Cutting Board",
                Description = """
                    A premium bamboo cutting board with a deep juice groove along the perimeter to catch liquids during food prep.
                    Features a built-in herb stripper, a hanging hole for easy storage, and a non-slip base to keep it firmly in place.
                    Naturally antimicrobial and gentle on knife edges, making it an essential kitchen companion.
                    """,
            },
            new Item
            {
                Id = 3,
                Name = "Ceramic Pour-Over Coffee Dripper",
                Description = """
                    A handcrafted ceramic pour-over coffee dripper paired with a solid walnut stand and a reusable stainless mesh filter.
                    The carefully angled interior ribs slow the flow rate for optimal extraction, producing a clean and nuanced cup.
                    Designed for coffee enthusiasts who treat their morning brew as a ritual rather than a routine.
                    """,
            },
            new Item
            {
                Id = 4,
                Name = "Merino Wool Travel Blanket",
                Description = """
                    A ultra-soft Merino wool travel blanket that folds and compresses neatly into its own integrated carry pouch.
                    Naturally temperature-regulating and odor-resistant, it keeps you warm on cold flights and cool in mild conditions.
                    Lightweight enough to slip into a carry-on, yet substantial enough to replace a full-size throw at home.
                    """,
            },
            new Item
            {
                Id = 5,
                Name = "Solar-Powered Desk Lamp",
                Description = """
                    A solar-powered desk lamp with a flexible gooseneck arm, adjustable color temperature ranging from warm amber to cool daylight,
                    and a built-in USB-A charging port to keep your devices powered throughout the day.
                    The wide solar panel charges efficiently even in indirect light, making it ideal for eco-conscious home and office setups.
                    """,
            },
        };

        context.Products.AddRange(products);
        context.Items.AddRange(items);

        await context.SaveChangesAsync();
    }
}
