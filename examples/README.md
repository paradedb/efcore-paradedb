# ParadeDB for .NET: Examples & Cookbook

This directory contains self-contained .NET console example projects that demonstrate how to use ParadeDB with EF Core.
Each example is a minimal, runnable .NET app with its own `appsettings.json`, migrations, and a `Program.cs` entry point.

## 🚀 Getting Started

Prerequisites

- .NET 10 SDK
- Docker (to run the ParadeDB Docker image)

Start ParadeDB

From the repo root you can start a local ParadeDB container with the included helper script:

```bash
./scripts/run_paradedb.sh
```

If you already run PostgreSQL with ParadeDB installed, update the `Default` connection string in the example's `appsettings.json`.

## 📚 The Examples

We've organized the examples into three categories:

1. **Essentials**: Core search features used in almost every app.
2. **Smart Features**: UX enhancements like autocomplete and recommendations.
3. **AI & Vectors**: Advanced semantic search and generative AI flows.

### 🔹 Essentials

#### 1. Quickstart (`examples/Quickstart`)

_The "Hello World" of ParadeDB._

This script demonstrates the fundamental building blocks of search. You will learn how to:

- **Index data**: Define a `BM25Index` on your model.
- **Search**: Perform basic keyword queries.
- **Score**: Sort results by relevance (BM25 score).
- **Highlight**: Generate snippets (e.g., `<b>run</b>ning`) to show users why a result matched.

**Run it:**

```bash
dotnet run --project examples/Quickstart
```

#### 2. Faceted Search (`examples/FacetedSearch`)

_Building an E-commerce Sidebar._

Facets are the "filters" you see on shopping sites (e.g., "Brand (5)", "Color (3)"). This example shows how to compute these counts efficiently in a single query.

**Key Concepts:**

- **Aggregations**: Counting documents by category, rating, etc.
- **Hybrid Results**: Getting search results _and_ facet counts together.

**Run it:**

```bash
dotnet run --project examples/FacetedSearch
```

---

### 🔹 Smart Features

#### 3. Autocomplete (`examples/Autocomplete`)

_Instant "As-You-Type" Suggestions._

Standard search requires hitting "Enter". Autocomplete gives immediate feedback. This example uses **N-gram tokenization** to match substrings (e.g., "wir" matches "wireless").

**How it works:**

1. We create a specialized index that breaks text into small chunks (n-grams).
2. Queries match these chunks, allowing for partial matches even in the middle of words.

**Run it:**

```bash
dotnet run --project examples/Autocomplete
```

#### 4. More Like This (`examples/MoreLikeThis`)

_Recommendations & "Related Content"._

Want to show "Related Articles" or "Customers also bought"? This feature analyzes the text of a document to find others with similar keywords, using TF-IDF logic-no complex vector embeddings required.

**Run it:**

```bash
dotnet run --project examples/MoreLikeThis
```

---

### 🔹 AI & Vectors

#### 5. Hybrid Search with RRF (`examples/HybridRrf`)

_The Best of Both Worlds: Keywords + Semantics._

Keyword search (BM25) is great for exact matches ("Part #123"). Vector search is great for meaning ("warm clothing" matches "coat"). **Hybrid Search** combines them using **Reciprocal Rank Fusion (RRF)** for superior results.

**Run it:**

```bash
dotnet run --project examples/HybridRrf
```

#### 6. RAG: Retrieval-Augmented Generation (`examples/Rag`)

_Chat with your Data._

This example builds a mini QA system. It searches your data for relevant context and feeds it to an LLM (Large Language Model) to answer questions based _only_ on your data.

**Prerequisites:**

- An API Key from [OpenRouter](https://openrouter.ai/) (provides access to GPT-4, Claude, etc.).
- Set the key with .NET user secrets for this project.

**Run it:**

```bash
dotnet user-secrets set "OpenRouter:ApiKey" "sk-..." --project examples/Rag
dotnet run --project examples/Rag
```
