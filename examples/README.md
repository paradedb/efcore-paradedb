# Examples

This directory contains EF Core examples for ParadeDB. Run all of them with `scripts/run_examples.sh`, or choose one below.

## Quickstart (`examples/Quickstart`)

The "Hello World" of ParadeDB. Demonstrates basic keyword search, relevance
scoring, and highlighting.

```bash
dotnet run --project examples/Examples.csproj -p:Example=Quickstart
```

## Faceted Search (`examples/FacetedSearch`)

Builds an e-commerce-style filtering sidebar with facet counts alongside
search results.

```bash
dotnet run --project examples/Examples.csproj -p:Example=FacetedSearch
```

## Autocomplete (`examples/Autocomplete`)

Uses n-gram tokenization to provide as-you-type suggestions and partial word
matches.

```bash
dotnet run --project examples/Examples.csproj -p:Example=Autocomplete
```

## More Like This (`examples/MoreLikeThis`)

Finds related content by analyzing document text for similar keywords, without
requiring vector embeddings.

```bash
dotnet run --project examples/Examples.csproj -p:Example=MoreLikeThis
```

## Hybrid Search with RRF (`examples/HybridRrf`)

Combines keyword and vector search with reciprocal rank fusion for stronger
ranking.

```bash
dotnet run --project examples/Examples.csproj -p:Example=HybridRrf
```

## RAG: Retrieval-Augmented Generation (`examples/Rag`)

Builds a small QA flow that retrieves relevant context and sends it to an LLM
through OpenRouter.

Requires `OPENROUTER_API_KEY`.

```bash
dotnet run --project examples/Examples.csproj -p:Example=Rag
```
