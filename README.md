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
  <b>Search without a second system.</b><br/>
  One Postgres for your application data, full-text search, vector retrieval, and aggregations.
</p>

<h3 align="center">
  <a href="https://paradedb.com">Website</a> &bull;
  <a href="https://docs.paradedb.com">Docs</a> &bull;
  <a href="https://paradedb.com/slack/">Community</a> &bull;
  <a href="https://paradedb.com/blog/">Blog</a> &bull;
  <a href="https://docs.paradedb.com/changelog/">Changelog</a>
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

The official [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) integration for [ParadeDB](https://paradedb.com) (powered by the [`pg_search`](https://github.com/paradedb/paradedb) Postgres extension), including first-class support for managing ParadeDB indexes and running queries using the full ParadeDB API. Follow the [getting started guide](https://docs.paradedb.com/documentation/getting-started/environment#entity-framework-core) to begin.

## Requirements & Compatibility

| Component  | Supported                     |
| ---------- | ----------------------------- |
| .NET       | 8.0+                          |
| EF Core    | 8.0+                          |
| ParadeDB   | 0.25.0+                       |
| PostgreSQL | 15+ (with ParadeDB extension) |

## Vector Search

ParadeDB indexes pgvector `vector` columns directly inside its BM25 index — no
pgvector ORM plugin is required. Map a `float[]` property to a `vector(n)`
column, pick a distance metric for the index, and order by the matching
distance function:

```csharp
public class Item
{
    public int Id { get; set; }
    public float[]? Embedding { get; set; }
}

// Model configuration
modelBuilder.Entity<Item>(entity =>
{
    entity.Property(x => x.Embedding).HasVectorType(384);

    entity
        .HasParadeDbIndex("items_idx", x => x.Id)
        .HasField(x => x.Embedding, VectorMetric.Cosine);
});

// Top-K query: a `@@@` predicate (`EF.Functions.All` for match-all here) and
// a LIMIT (`Take`) are required for the index to serve the query
float[] queryEmbedding = GetQueryEmbedding();

var results = await db
    .Items.Where(x => EF.Functions.All(x.Id))
    .OrderBy(x => EF.Functions.CosineDistance(x.Embedding, queryEmbedding))
    .Take(10)
    .ToListAsync();
```

`VectorMetric.L2` (`<->`, the default), `VectorMetric.Cosine` (`<=>`), and
`VectorMetric.InnerProduct` (`<#>`) map to the `vector_l2_ops`,
`vector_cosine_ops`, and `vector_ip_ops` operator classes. The distance
function used in `OrderBy` must match the metric of the index opclass —
`L2Distance` with `L2`, `CosineDistance` with `Cosine`, and `InnerProduct`
with `InnerProduct` — otherwise the query still returns correct results but
falls back to a sequential scan instead of Top-K index pushdown.

## Examples

- [Quickstart](examples/Quickstart/Program.cs)
- [Faceted Search](examples/FacetedSearch/Program.cs)
- [Autocomplete](examples/Autocomplete/Program.cs)
- [More Like This](examples/MoreLikeThis/Program.cs)
- [Hybrid Search (RRF)](examples/HybridRrf/Program.cs)
- [Vector Search](examples/VectorSearch/Program.cs)
- [RAG](examples/Rag/Program.cs)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup, running tests, linting, and the PR workflow.

## Support

If you're missing a feature or have found a bug, please open a
[GitHub Issue](https://github.com/paradedb/efcore-paradedb/issues/new/choose).

To get community support, you can:

- Post a question in the [ParadeDB Slack Community](https://paradedb.com/slack)
- Ask for help on our [GitHub Discussions](https://github.com/paradedb/paradedb/discussions)

If you need commercial support, please [contact the ParadeDB team](mailto:sales@paradedb.com).

## Acknowledgments

We would like to thank the following members of the Entity Framework Core community:

- [Nandor Krizbai](https://github.com/nandor23) - for the initial implementation of this project
- [Daniel Oliveira](https://github.com/daniel3303) - for implementing [ParadeDbEntityFrameworkCore](https://github.com/daniel3303/ParadeDbEntityFrameworkCore) which inspired our indexing implementation

## License

ParadeDB for Entity Framework Core is licensed under the [MIT License](LICENSE).
