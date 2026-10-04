# ParadeDB for Entity Framework Core

The official [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) integration for ParadeDB. Get started with the [setup guide](https://www.paradedb.com/docs/start/connect-your-app#ef-core).

## Requirements & Compatibility

| Component            | Supported                                                                                                                                                            |
| -------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| .NET                 | 8.0+                                                                                                                                                                 |
| EF Core              | 8.0+                                                                                                                                                                 |
| PostgreSQL           | 15+                                                                                                                                                                  |
| pgvector             | 0.7.0+ (provides vector types for ParadeDB’s [native vector search](https://www.paradedb.com/docs/reference/vector/overview), included in the ParadeDB Docker image) |
| ParadeDB / pg_search | 0.26.0+                                                                                                                                                              |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for development setup, running tests, linting, and the PR workflow.

## Support

If you're missing a feature or have found a bug, please open a [GitHub Issue](https://github.com/paradedb/efcore-paradedb/issues/new/choose). For community support, join the [ParadeDB Slack Community](https://paradedb.com/slack).

## Acknowledgments

We would like to thank the following members of the Entity Framework Core community:

- [Nandor Krizbai](https://github.com/nandor23) - for the initial implementation of this project
- [Daniel Oliveira](https://github.com/daniel3303) - for implementing [ParadeDbEntityFrameworkCore](https://github.com/daniel3303/ParadeDbEntityFrameworkCore), which inspired our indexing implementation

## License

ParadeDB for Entity Framework Core is licensed under the [MIT License](LICENSE).
