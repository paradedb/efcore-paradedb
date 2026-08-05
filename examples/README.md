# Examples

Self-contained programs that show how to use ParadeDB from Entity Framework Core. Run them all with `scripts/run_examples.sh`, or follow the setup below and run them one at a time.

## Getting Started

### 1. Install dependencies

```bash
dotnet tool restore
dotnet restore build.slnf
```

### 2. Start ParadeDB

```bash
source scripts/run_paradedb.sh
```

This starts a ParadeDB container via Docker and exports `DATABASE_URL`. If you already have a Postgres instance with ParadeDB installed, set `DATABASE_URL` yourself instead:

```bash
export DATABASE_URL=postgresql://user:password@localhost:5432/dbname
```

## Quickstart (`Quickstart/Program.cs`)

The "Hello World" of ParadeDB. Covers declaring a ParadeDB index on an entity, running keyword queries, sorting by BM25 relevance, and highlighting matched terms in snippets.

```bash
dotnet run --project examples/Examples.csproj -p:Example=Quickstart
```

## Vector Search (`VectorSearch/Program.cs`)

Top-K nearest-neighbor retrieval over pgvector `vector` columns. ParadeDB indexes the vector column inside its search index, so one index serves both keyword and vector queries.

Requires the `pgvector` extension, which is included in the ParadeDB Docker image.

```bash
dotnet run --project examples/Examples.csproj -p:Example=VectorSearch
```

## Faceted Search (`FacetedSearch/Program.cs`)

Builds an e-commerce-style filter sidebar. Computes search results and facet counts (by category, rating, and so on) together in a single query.

```bash
dotnet run --project examples/Examples.csproj -p:Example=FacetedSearch
```

## Autocomplete (`Autocomplete/Program.cs`)

As-you-type suggestions using n-gram tokenization, which matches substrings in the middle of words — typing `wir` matches `wireless`.

```bash
dotnet run --project examples/Examples.csproj -p:Example=Autocomplete
```

## More Like This (`MoreLikeThis/Program.cs`)

"Related content" recommendations. Finds documents with similar keywords using TF-IDF logic, without requiring vector embeddings.

```bash
dotnet run --project examples/Examples.csproj -p:Example=MoreLikeThis
```

## Hybrid Search (RRF) (`HybridRrf/Program.cs`)

Combines BM25 keyword search (good for exact matches like part numbers) with vector similarity (good for meaning) using Reciprocal Rank Fusion, which ranks better than either method alone.

Requires the `pgvector` extension, which is included in the ParadeDB Docker image.

```bash
dotnet run --project examples/Examples.csproj -p:Example=HybridRrf
```

## RAG (`Rag/Program.cs`)

A small question-answering flow. Retrieves relevant context with ParadeDB, then sends it to an LLM so answers are grounded in your own data.

Requires an [OpenRouter](https://openrouter.ai/) API key:

```bash
export OPENROUTER_API_KEY=sk-...
dotnet run --project examples/Examples.csproj -p:Example=Rag
```

## Shared Helpers (`Shared/`)

The examples share the `Shared/` project, which holds the demo `DbContext`, the entity types used across examples, and the seeding helpers.
