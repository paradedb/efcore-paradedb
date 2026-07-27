# ParadeDB for Entity Framework Core

The official [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) integration for [ParadeDB](https://paradedb.com), including first-class support for managing ParadeDB indexes and running queries using the full ParadeDB API. The integration covers both full-text search and [vector search](https://docs.paradedb.com/documentation/vector/overview) over pgvector `vector` types. Follow the [getting started guide](https://docs.paradedb.com/documentation/getting-started/environment#entity-framework-core) to begin.

## Requirements & Compatibility

| Component  | Supported                                                         |
| ---------- | ----------------------------------------------------------------- |
| .NET       | 8.0+                                                              |
| EF Core    | 8.0+                                                              |
| ParadeDB   | 0.25.0+                                                           |
| PostgreSQL | 15+ (with ParadeDB extension)                                     |
| pgvector   | Required for vector search; included in the ParadeDB Docker image |

## Examples

- [Quickstart](examples/Quickstart/Program.cs)
- [Vector Search](examples/VectorSearch/Program.cs)
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

We would like to thank the following members of the Entity Framework Core community:

- [Nandor Krizbai](https://github.com/nandor23) - for the initial implementation of this project
- [Daniel Oliveira](https://github.com/daniel3303) - for implementing [ParadeDbEntityFrameworkCore](https://github.com/daniel3303/ParadeDbEntityFrameworkCore) which inspired our indexing implementation

## License

ParadeDB for Entity Framework Core is licensed under the [MIT License](LICENSE).
