# ParadeDB for Entity Framework Core

The official [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) integration for [ParadeDB](https://paradedb.com). Follow the [getting started guide](https://www.paradedb.com/docs/start/connect-your-app#ef-core) to begin.

## Requirements & Compatibility

| Component  | Supported                                                         |
| ---------- | ----------------------------------------------------------------- |
| .NET       | 8.0+                                                              |
| EF Core    | 8.0+                                                              |
| ParadeDB   | 0.25.0+                                                           |
| PostgreSQL | 15+ (with ParadeDB extension)                                     |
| pgvector   | Required for vector search; included in the ParadeDB Docker image |

## Examples

Follow the [example setup guide](https://www.paradedb.com/docs/guides/setup), then select the EF Core tab in a guide:

- [Quickstart](https://www.paradedb.com/docs/guides/quickstart)
- [Vector Search](https://www.paradedb.com/docs/guides/vector-search)
- [Faceted Search](https://www.paradedb.com/docs/guides/faceted-search)
- [Autocomplete](https://www.paradedb.com/docs/guides/search-as-you-type)
- [More Like This](https://www.paradedb.com/docs/guides/more-like-this)
- [Hybrid Search (RRF)](https://www.paradedb.com/docs/guides/hybrid-search)
- [RAG](https://www.paradedb.com/docs/guides/rag-and-agents)

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
