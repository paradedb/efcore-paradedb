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
  <b>Just use Postgres.</b><br/>
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

The official [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) integration for [ParadeDB](https://paradedb.com) (powered by the [`pg_search`](https://github.com/paradedb/paradedb) Postgres extension). Follow the [getting started guide](https://www.paradedb.com/docs/start/connect-your-app#ef-core) to begin.

## Requirements & Compatibility

| Component  | Supported                                                          |
| ---------- | ------------------------------------------------------------------ |
| .NET       | 8.0+                                                               |
| EF Core    | 8.0+                                                               |
| ParadeDB   | 0.25.0+                                                            |
| PostgreSQL | 15+ (with the ParadeDB pg_search extension)                        |
| pgvector   | Required for vector search (included in the ParadeDB Docker image) |

## Examples

Follow the [example setup guide](https://www.paradedb.com/docs/guides/setup), then choose a guide and select the EF Core tab:

- [Quickstart](https://www.paradedb.com/docs/start/connect-your-app)
- [Vector Search](https://www.paradedb.com/docs/guides/vector-search)
- [Faceted Search](https://www.paradedb.com/docs/guides/faceted-search)
- [Hybrid Search (RRF)](https://www.paradedb.com/docs/guides/hybrid-search)
- [Retrieval-Augmented Generation (RAG)](https://www.paradedb.com/docs/guides/rag-and-agents)
- [Autocomplete](https://www.paradedb.com/docs/guides/search-as-you-type)
- [More Like This](https://www.paradedb.com/docs/guides/more-like-this)

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
