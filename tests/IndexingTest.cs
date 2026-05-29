using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using ParadeDB.EntityFrameworkCore.Extensions;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests;

public sealed class IndexingTest : TestBase
{
    private sealed class RegularIndexContext(DbContextOptions<RegularIndexContext> options)
        : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndexingItem>(entity =>
            {
                entity.ToTable("indexing_items");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Description).HasColumnName("description");

                entity.HasIndex(e => e.Description).HasDatabaseName("indexing_items_idx");
            });
        }
    }

    // We include a regular index here to check that the code changes to support bm25 indexes aren't inadvertently
    // breaking regular ones
    [Test]
    public async Task RegularIndex()
    {
        await using var context = DbFixture.CreateContext();
        await context.Database.OpenConnectionAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TEMP TABLE indexing_items (
                id int PRIMARY KEY,
                description text
            );
            """
        );

        var sql = GenerateCreateIndexSql<RegularIndexContext, IndexingItem>();

        sql.ShouldBe(
            """
            CREATE INDEX indexing_items_idx ON indexing_items (description);

            """
        );
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private sealed class JsonExpressionIndexContext(
        DbContextOptions<JsonExpressionIndexContext> options
    ) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndexingItem>(entity =>
            {
                entity.ToTable("indexing_items");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Metadata).HasColumnName("metadata");
                entity.Property(e => e.Rating).HasColumnName("rating");

                entity
                    .HasBm25Index("indexing_items_idx", e => e.Id)
                    .HasField(
                        e => e.Description,
                        Tokenizer.Ngram(3, 3, new() { ["positions"] = true })
                    )
                    .HasField(
                        "metadata ->> 'color'",
                        Tokenizer.Literal(new() { ["alias"] = "metadata_color" })
                    )
                    .HasField(e => e.Rating, new FieldAlias("my_rating_alias"))
                    .HasField("rating + 1", new FieldAlias("escape' me"))
                    .HasFilter("rating > 0")
                    .HasSearchTokenizer(Tokenizer.Simple(new() { ["lowercase"] = false }));
            });
        }
    }

    [Test]
    public async Task Bm25Index_WithJsonExpressionTokenizerOptionsAndFilter()
    {
        await using var context = DbFixture.CreateContext();
        await context.Database.OpenConnectionAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TEMP TABLE indexing_items (
                id int PRIMARY KEY,
                description text,
                metadata jsonb,
                rating int
            );
            """
        );

        var sql = GenerateCreateIndexSql<JsonExpressionIndexContext, IndexingItem>();

        sql.ShouldBe(
            """
            CREATE INDEX indexing_items_idx ON indexing_items USING bm25 (id, (description::pdb.ngram(3,3,'positions=true')), ((metadata ->> 'color')::pdb.literal('alias=metadata_color')), (rating::pdb.alias('my_rating_alias')), ((rating + 1)::pdb.alias('escape'' me'))) WITH (key_field = 'id', search_tokenizer = 'simple(lowercase=false)') WHERE rating > 0;

            """
        );
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private sealed class SimpleIndexContext(DbContextOptions<SimpleIndexContext> options)
        : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndexingItem>(entity =>
            {
                entity.ToTable("indexing_items");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Metadata).HasColumnName("metadata");
                entity.Property(e => e.Rating).HasColumnName("rating");

                entity
                    .HasBm25Index("indexing_items_idx", e => e.Id)
                    .IsCreatedConcurrently()
                    .HasField(e => e.Description)
                    .HasField(e => e.Metadata);
            });
        }
    }

    [Test]
    public async Task SimpleBm25Index()
    {
        await using var context = DbFixture.CreateContext();
        await context.Database.OpenConnectionAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TEMP TABLE indexing_items (
                id int PRIMARY KEY,
                description text,
                metadata jsonb,
                rating int
            );
            """
        );

        var sql = GenerateCreateIndexSql<SimpleIndexContext, IndexingItem>();

        sql.ShouldBe(
            """
            CREATE INDEX CONCURRENTLY indexing_items_idx ON indexing_items USING bm25 (id, description, metadata) WITH (key_field = 'id');

            """
        );
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private sealed class ArrayExpressionIndexContext(
        DbContextOptions<ArrayExpressionIndexContext> options
    ) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndexingItem>(entity =>
            {
                entity.ToTable("indexing_items");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.Category).HasColumnName("category");
                entity.Property(e => e.Categories).HasColumnName("categories");
                entity.Property(e => e.Tags).HasColumnName("tags");

                entity
                    .HasBm25Index("indexing_items_idx", e => e.Id)
                    .HasField(e => e.Categories)
                    .HasField(e => e.Tags, Tokenizer.Literal())
                    .HasField(
                        "description || ' ' || category",
                        Tokenizer.Simple(new() { ["alias"] = "description_concat" })
                    )
                    .HasField(e => e.Description, Tokenizer.Literal())
                    .HasField(
                        e => e.Description,
                        Tokenizer.Simple(new() { ["alias"] = "description_simple" })
                    );
            });
        }
    }

    [Test]
    public async Task Bm25Index_WithArrayExpressionAndMultipleTokenizers()
    {
        await using var context = DbFixture.CreateContext();
        await context.Database.OpenConnectionAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TEMP TABLE indexing_items (
                id int PRIMARY KEY,
                description text,
                category text,
                categories text[],
                tags varchar(255)[]
            );
            """
        );

        var sql = GenerateCreateIndexSql<ArrayExpressionIndexContext, IndexingItem>();

        sql.ShouldBe(
            """
            CREATE INDEX indexing_items_idx ON indexing_items USING bm25 (id, categories, (tags::pdb.literal), ((description || ' ' || category)::pdb.simple('alias=description_concat')), (description::pdb.literal), (description::pdb.simple('alias=description_simple'))) WITH (key_field = 'id');

            """
        );
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    public static IEnumerable<(
        Tokenizer Tokenizer,
        string ExpectedSearchTokenizer
    )> SearchTokenizerVariations()
    {
        yield return (
            Tokenizer.Unicode(new() { ["remove_emojis"] = true }),
            "unicode_words(remove_emojis=true)"
        );
        yield return (
            Tokenizer.Simple(
                new()
                {
                    ["stemmer"] = "english",
                    ["max_token_length"] = 255,
                    ["threshold"] = 2.3f,
                }
            ),
            "simple(stemmer=english,max_token_length=255,threshold=2.3)"
        );
        yield return (Tokenizer.Icu(), "icu");
        yield return (Tokenizer.ChineseCompatible([]), "chinese_compatible");
        yield return (Tokenizer.Jieba(), "jieba");
        yield return (Tokenizer.Literal(), "literal");
        yield return (
            Tokenizer.LiteralNormalized(new() { ["trim"] = true }),
            "literal_normalized(trim=true)"
        );
        yield return (
            Tokenizer.Ngram(3, 3, new() { ["positions"] = true, ["prefix_only"] = true }),
            "ngram(3,3,positions=true,prefix_only=true)"
        );
        yield return (
            Tokenizer.EdgeNgram(2, 5, new() { ["token_chars"] = "letter,digit,punctuation" }),
            "edge_ngram(2,5,token_chars=letter,digit,punctuation)"
        );
        yield return (Tokenizer.RegexPattern("isn't", []), "regex_pattern(''isn''''t'')");
        yield return (Tokenizer.SourceCode(), "source_code");
        yield return (Tokenizer.Whitespace(), "whitespace");
    }

    private sealed class SearchTokenizerIndexContext(
        DbContextOptions<SearchTokenizerIndexContext> options,
        Tokenizer tokenizer
    ) : DbContext(options)
    {
        public Tokenizer Tokenizer { get; } = tokenizer;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndexingItem>(entity =>
            {
                entity.ToTable("indexing_items");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Description).HasColumnName("description");

                entity
                    .HasBm25Index("indexing_items_idx", e => e.Id)
                    .HasField(e => e.Description)
                    .HasSearchTokenizer(Tokenizer);
            });
        }
    }

    private sealed class SearchTokenizerIndexModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime) =>
            context is SearchTokenizerIndexContext searchTokenizerContext
                ? (context.GetType(), searchTokenizerContext.Tokenizer.ToString(), designTime)
                : (context.GetType(), designTime);

        public object Create(DbContext context) => Create(context, false);
    }

    [Test]
    [MethodDataSource(nameof(SearchTokenizerVariations))]
    public async Task Bm25Index_SearchTokenizerRendersTokenizer(
        Tokenizer tokenizer,
        string expectedSearchTokenizer
    )
    {
        await using var context = DbFixture.CreateContext();
        await context.Database.OpenConnectionAsync();
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TEMP TABLE indexing_items (
                id int PRIMARY KEY,
                description text
            );
            """
        );

        var sql = GenerateSearchTokenizerCreateIndexSql(tokenizer);

        sql.ShouldBe(
            $"CREATE INDEX indexing_items_idx ON indexing_items USING bm25 (id, description) WITH (key_field = 'id', search_tokenizer = '{expectedSearchTokenizer}');\n"
        );
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static string GenerateCreateIndexSql<TContext, TEntity>()
        where TContext : DbContext
    {
        using var context = (TContext)
            Activator.CreateInstance(
                typeof(TContext),
                new DbContextOptionsBuilder<TContext>()
                    .UseNpgsql("Host=localhost;Database=test", o => o.UseParadeDb())
                    .Options
            )!;
        var model = context.GetService<IDesignTimeModel>().Model;
        var index = model
            .FindEntityType(typeof(TEntity))!
            .GetIndexes()
            .Single(i => i.GetDatabaseName() == "indexing_items_idx")
            .GetMappedTableIndexes()
            .Single();

        return context
            .GetService<IMigrationsSqlGenerator>()
            .Generate([CreateIndexOperation.CreateFrom(index)], model)
            .Single()
            .CommandText;
    }

    private static string GenerateSearchTokenizerCreateIndexSql(Tokenizer tokenizer)
    {
        using var context = new SearchTokenizerIndexContext(
            new DbContextOptionsBuilder<SearchTokenizerIndexContext>()
                .UseNpgsql("Host=localhost;Database=test", o => o.UseParadeDb())
                .ReplaceService<IModelCacheKeyFactory, SearchTokenizerIndexModelCacheKeyFactory>()
                .Options,
            tokenizer
        );
        var model = context.GetService<IDesignTimeModel>().Model;
        var index = model
            .FindEntityType(typeof(IndexingItem))!
            .GetIndexes()
            .Single(i => i.GetDatabaseName() == "indexing_items_idx")
            .GetMappedTableIndexes()
            .Single();

        return context
            .GetService<IMigrationsSqlGenerator>()
            .Generate([CreateIndexOperation.CreateFrom(index)], model)
            .Single()
            .CommandText;
    }

    private sealed class IndexingItem
    {
        public int Id { get; set; }
        public string Description { get; set; } = "";
        public string Category { get; set; } = "";
        public string[] Categories { get; set; } = [];
        public string[] Tags { get; set; } = [];
        public JsonDocument? Metadata { get; set; }
        public int Rating { get; set; }
    }
}
