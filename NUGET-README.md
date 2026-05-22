# ParadeDB.EntityFrameworkCore

Entity Framework Core extension for [ParadeDB](https://www.paradedb.com) pg_search

[![Build](https://github.com/paradedb/efcore-paradedb/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/paradedb/efcore-paradedb/actions/workflows/ci.yml)
[![License](https://img.shields.io/github/license/paradedb/efcore-paradedb?color=%231e8e7e)](https://opensource.org/license/mit)

ParadeDB.EntityFrameworkCore adds support for ParadeDB's pg_search extension to [Npgsql.EntityFrameworkCore.PostgreSQL](https://www.npgsql.org/efcore/index.html?tabs=onconfiguring), exposing ParadeDB search functions through the **EF.Functions** API for LINQ-based full-text search queries.
BM25 index creation must be defined using raw SQL. See the [EF Core documentation](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli#adding-raw-sql) for details on adding raw SQL to migrations.

## Requirements & Compatibility

| Component  | Supported                     |
| ---------- | ----------------------------- |
| .NET       | 8+                            |
| EF Core    | 8+                            |
| ParadeDB   | 0.22.0+                       |
| PostgreSQL | 15+ (with ParadeDB extension) |

## Configuration

Install the [Npgsql.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL/10.0.0-rc.1#readme-body-tab)
NuGet package and configure your DbContext by calling `UseParadeDb()` on the `NpgsqlDbContextOptionsBuilder` to enable pg_search function mappings.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextPool<AppDbContext>(opt =>
{
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("AppDatabase"),
        o => o.UseParadeDb()
    );
});
```

## Function Mappings

The following ParadeDB operations are available through the `EF.Functions` API:

| ParadeDB Operation                                                          | LINQ Methods               |
| --------------------------------------------------------------------------- | -------------------------- |
| [Match](https://docs.paradedb.com/documentation/full-text/match)            | `MatchAny()`, `MatchAll()` |
| [Phrase](https://docs.paradedb.com/documentation/full-text/phrase)          | `Phrase()`                 |
| [Term](https://docs.paradedb.com/documentation/full-text/term)              | `Term()`                   |
| [Highlighting](https://docs.paradedb.com/documentation/full-text/highlight) | `Snippet()`                |
| [Proximity](https://docs.paradedb.com/documentation/full-text/proximity)    | `Proximity()`              |
| [BM25 scoring](https://docs.paradedb.com/documentation/sorting/score)       | `Score()`                  |
| [Tokenizers](https://docs.paradedb.com/documentation/tokenizers/overview)   | `TokenizeAsArray()`        |

## Usage Example

```csharp
var products = await dbContext
    .Products.Where(p =>
        EF.Functions.MatchAny(
            p.Description,
            "with shoes and",
            Pdb.Fuzzy(1),
            Pdb.Boost(2.3f)
        )
    )
    .Select(p => new
    {
        p.Id,
        p.Description,
        Score = EF.Functions.Score(p.Id),
    })
    .ToListAsync();
```

### Translates to

```sql
SELECT p.id AS "Id", p.description AS "Description", pdb.score(p.id) AS "Score"
FROM products AS p
WHERE p.description ||| 'with shoes and'::pdb.fuzzy(1)::pdb.boost(2.3)
```
