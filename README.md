<!-- ParadeDB: Postgres for Search and Analytics -->
<h1 align="center">
  <a href="https://paradedb.com"><img src="https://github.com/paradedb/paradedb/raw/main/docs/logo/readme.svg" alt="ParadeDB"></a>
<br>
</h1>

<p align="center">
  <b>Simple, Elastic-quality search for Postgres</b><br/>
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

> [!NOTE]
> `efcore-paradedb` is a work in progress and is not yet recommended for production use. APIs may evolve as we stabilize the integration — pin to an exact version if you're trying it out.

## ParadeDB for Entity Framework Core

The official [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) integration for [ParadeDB](https://paradedb.com), including first-class support for managing BM25 indexes and running queries using the full ParadeDB API. Follow the [getting started guide](https://docs.paradedb.com/documentation/getting-started/environment#efcore) to begin.

## Requirements & Compatibility

| Component  | Supported                     |
| ---------- | ----------------------------- |
| .NET       | 8+                            |
| EF Core    | 8+                            |
| ParadeDB   | 0.22.0+                       |
| PostgreSQL | 15+ (with ParadeDB extension) |

## Examples

- [Quickstart](examples/Quickstart/Program.cs)
- [Faceted Search](examples/FacetedSearch/Program.cs)
- [Autocomplete](examples/Autocomplete/Program.cs)
- [More Like This](examples/MoreLikeThis/Program.cs)
- [Hybrid Search (RRF)](examples/HybridRrf/Program.cs)
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

We would like to thank the following members of the Entity Framework Core community for the initial implementation of this project:

- [Nandor Krizbai](https://github.com/nandor23) - .NET developer

## License

ParadeDB for Entity Framework Core is licensed under the [MIT License](LICENSE).
