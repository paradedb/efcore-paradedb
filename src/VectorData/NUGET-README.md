# ParadeDB for Microsoft VectorData

Cosine vector search for .NET applications that use Microsoft's
`IVectorSearchable<TRecord>`. The adapter runs over your existing
[ParadeDB Entity Framework Core query](https://www.nuget.org/packages/ParadeDB.EntityFrameworkCore).
Your `DbContext` handles writes, migrations, and transactions.

## Install

```sh
dotnet add package ParadeDB.VectorData
```

This installs the matching EF Core package as a dependency. See the
[EF Core package](https://www.nuget.org/packages/ParadeDB.EntityFrameworkCore)
for compatibility and the EF Core setup guide.

## Search

Map a `float[]` property to a PostgreSQL `vector` column and include it in a
ParadeDB index with `vector_cosine_ops`. Use a ParadeDB predicate such as
`EF.Functions.All` on the index key to enable index-backed ranking.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.VectorData;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.VectorData;

IVectorSearchable<Document> search = new ParadeDbVectorSearch<Document>(
    db.Documents.Where(document => EF.Functions.All(document.Id)),
    nameof(Document.Embedding),
    document => new Document { Id = document.Id, Title = document.Title }
);

await foreach (var hit in search.SearchAsync(queryEmbedding, top: 10))
{
    Console.WriteLine($"{hit.Record.Title}: {hit.Score}");
}
```

Pass a precomputed query embedding. Results are ranked by cosine similarity,
with higher scores first. The projection controls which fields are returned
when embeddings are omitted.
