<h1 align="center">
  <a href="https://paradedb.com">
    <picture align=center>
      <source media="(prefers-color-scheme: dark)" srcset="https://github.com/paradedb/paradedb/raw/main/docs/logo/paradedb-logo-dark-large.svg">
      <source media="(prefers-color-scheme: light)" srcset="https://github.com/paradedb/paradedb/raw/main/docs/logo/paradedb-logo-light-large.svg">
      <img alt="The ParadeDB logo." src="https://github.com/paradedb/paradedb/raw/main/docs/logo/paradedb-logo-light-large.svg">
    </picture>
  </a>
  <br>
</h1>

<p align="center">
  <b>Just use Postgres</b><br/>
  One Postgres for your application data, full-text search, vector retrieval, and aggregations.
</p>

<h3 align="center">
  <a href="https://paradedb.com">Website</a> &bull;
  <a href="https://www.paradedb.com/docs/start/introduction">Docs</a> &bull;
  <a href="https://paradedb.com/slack/">Community</a> &bull;
  <a href="https://paradedb.com/blog/">Blog</a> &bull;
  <a href="https://www.paradedb.com/docs/project/changelog">Changelog</a>
</h3>

<p align="center">
  <a href="https://www.nuget.org/packages/ParadeDB.EntityFrameworkCore"><img src="https://img.shields.io/nuget/v/ParadeDB.EntityFrameworkCore" alt="NuGet Version"></a>&nbsp;
  <a href="https://www.nuget.org/packages/ParadeDB.EntityFrameworkCore"><img src="https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-blue?logo=dotnet" alt=".NET Versions"></a>&nbsp;
  <a href="https://www.nuget.org/packages/ParadeDB.EntityFrameworkCore"><img src="https://img.shields.io/nuget/dt/ParadeDB.EntityFrameworkCore" alt="NuGet Downloads"></a>&nbsp;
  <a href="https://codecov.io/gh/paradedb/efcore-paradedb"><img src="https://codecov.io/gh/paradedb/efcore-paradedb/graph/badge.svg" alt="Codecov"></a>&nbsp;
  <a href="https://github.com/paradedb/efcore-paradedb?tab=MIT-1-ov-file#readme"><img src="https://img.shields.io/github/license/paradedb/efcore-paradedb?color=blue" alt="License"></a>&nbsp;
  <a href="https://paradedb.com/slack"><img src="https://img.shields.io/badge/Join%20Slack-purple?logo=slack" alt="Community"></a>&nbsp;
  <a href="https://x.com/paradedb"><img src="https://img.shields.io/twitter/url?url=https%3A%2F%2Ftwitter.com%2Fparadedb&label=Follow%20%40paradedb" alt="Follow @paradedb"></a>
</p>

---

## ParadeDB for Entity Framework Core

The official [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) integration for ParadeDB. Get started with the [setup guide](https://www.paradedb.com/docs/start/connect-your-app#ef-core).

## Requirements & Compatibility

| Component            | Supported                                                                                                                     |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| .NET                 | 8.0+                                                                                                                          |
| EF Core              | 8.0+                                                                                                                          |
| PostgreSQL           | 15+                                                                                                                           |
| pgvector             | 0.7.0+ (provides vector types for ParadeDB’s [native vector search](https://www.paradedb.com/docs/reference/vector/overview)) |
| ParadeDB / pg_search | 0.26.2+                                                                                                                       |

## VectorData search

Use `ParadeDbVectorSearch<TRecord>` when your retrieval code expects Microsoft's
`IVectorSearchable<TRecord>`. It runs a cosine search over an EF Core query.
Your existing `DbContext` still handles writes, migrations, and transactions.

Configure `Document.Embedding` as a `float[]` mapped to a PostgreSQL vector
column and include it in a ParadeDB index with `vector_cosine_ops`. Register
the adapter in the same DI scope as your context.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.VectorData;

services.AddScoped<IVectorSearchable<Document>>(services =>
{
    var db = services.GetRequiredService<AppDbContext>();
    return new ParadeDbVectorSearch<Document>(
        db.Documents.Where(document => EF.Functions.All(document.Id)),
        nameof(Document.Embedding),
        document => new Document { Id = document.Id, Title = document.Title }
    );
});
```

The source query needs a ParadeDB search predicate to use the index for
ranking. `EF.Functions.All` matches every row through the index. For filtered
search, include the filter columns in the index and use EF-translatable filters.
The projection selects which fields to return when embeddings are omitted.

```csharp
await foreach (var hit in search.SearchAsync(
    queryEmbedding,
    top: 10,
    options: new() { Filter = document => document.TenantId == tenantId },
    cancellationToken: cancellationToken
))
{
    Console.WriteLine($"{hit.Record.Title}: {hit.Score}");
}
```

Supply a precomputed `float[]` or `ReadOnlyMemory<float>` with the indexed
dimension. Scores are cosine similarity (`1 - cosine distance`), with higher
scores ranked first. The adapter supports `Filter` and `IncludeVectors`.
Setting `IncludeVectors` returns the full untracked entity.
`Skip`, `ScoreThreshold`, and `VectorProperty` are unsupported. Enumerate
results within the context's lifetime and complete or dispose the enumeration
before starting another operation on the same context.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup, running tests, linting, and the PR workflow.

## Support

If you're missing a feature or have found a bug, please open a [GitHub Issue](https://github.com/paradedb/efcore-paradedb/issues/new/choose). For community support, join the [ParadeDB Slack Community](https://paradedb.com/slack).

## Acknowledgments

We would like to thank the following members of the Entity Framework Core community for their valuable contributions during the development of this package:

- [Nandor Krizbai](https://github.com/nandor23) - Initial author of this project
- [Daniel Oliveira](https://github.com/daniel3303) - Creator of [ParadeDbEntityFrameworkCore](https://github.com/daniel3303/ParadeDbEntityFrameworkCore)

## License

ParadeDB for Entity Framework Core is licensed under the [MIT License](LICENSE).
