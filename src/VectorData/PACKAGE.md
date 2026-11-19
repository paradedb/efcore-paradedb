# ParadeDB VectorData

Use `ParadeDB.VectorData` when your .NET retrieval pipeline expects Microsoft's
`IVectorSearchable<TRecord>`. The adapter runs cosine search over an existing
ParadeDB EF Core query. Your `DbContext` continues to own writes, migrations,
and transactions.

Install `ParadeDB.VectorData` and use the same version of
`ParadeDB.EntityFrameworkCore`. Configure your `float[]` embedding as a
PostgreSQL vector column and include it in a ParadeDB index with
`vector_cosine_ops`. See the XML documentation on
`ParadeDB.VectorData.ParadeDbVectorSearch<TRecord>` for query requirements
and supported search options.
