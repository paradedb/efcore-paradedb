# Index and query options

## Partitioning and vector configuration

[ParadeDB 0.26.0](https://www.paradedb.com/docs/project/changelog/0.26.0) adds segment partitioning and quantized vector storage. Partition keys are comma-separated **index field names**, including tokenizer aliases when applicable. Each key must be single-valued and columnar. Numeric columns work directly; text keys need a columnar tokenizer such as `literal`. PostgreSQL validates the field types when creating the index.

```csharp
using ParadeDB.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;

entity.HasParadeDbIndex("items_search_idx", item => item.Id)
    .HasField(item => item.TenantId)
    .HasField(item => item.Description)
    .HasField(item => item.Embedding, VectorMetric.Cosine)
    .HasPartitionBy("TenantId")
    .HasTargetSegmentCount(8)
    .HasVectorQuantization("Embedding", false);

var totals = context.Items.Select(item =>
    EF.Functions.Agg(new { value_count = new { field = "Id" } }, "threshold"));
var config = await context.Database.VectorConfig("items_search_idx", "Embedding").ToListAsync();
```

EF migrations preserve these index annotations. The string visibility overload is available for `Agg`, `AggFilter`, `AggOver`, and `AggFilterOver`. For an explicit quantization schedule, use `.HasVectorQuantization("Embedding", 1, 4)`.

`target_segment_count` is a positive integer. Omit an option to retain the server default. Quantization can be disabled per vector field or configured with `{"quantization": {"layers": [1, 4]}}`. Quantization changes take effect at `CREATE INDEX` or `REINDEX`, so changing a reloption alone does not rebuild stored vectors.

## Aggregate visibility

- `transaction` applies transaction visibility checks and is the default.
- `raw` skips visibility checks and may include deleted or otherwise invisible rows.
- `threshold` applies checks only when the estimated match count is below `paradedb.visibility_threshold`.

The existing `exact` option remains supported. When using a named visibility option, omit the legacy boolean option.

Combining `FILTER` with a window aggregate is subject to server feature flags in 0.26.0.

## Vector diagnostics

Use `VectorInfo`, `VectorConfig`, and `VectorEstimatorInfo` to inspect vector storage, build configuration, and estimator error. Each accepts an index name and vector field name. The estimator helper also accepts an optional collection of query vectors. Names and queries are safely quoted or passed as SQL parameters.

Estimator diagnostics require at least one visible quantized IVF segment. An empty index or an index with only flat or unquantized segments returns a PostgreSQL error. Flat segments return null for IVF and quantization metadata where it does not apply. These are diagnostic operations, especially the estimator, and should not run on every application request.

## Tokenizer options

The existing tokenizer option dictionaries support the new options:

```csharp
Tokenizer.Simple(new() { ["pnorms"] = true });
Tokenizer.Jieba(new() { ["search_mode"] = false });
Tokenizer.ChineseCompatible(new() { ["chinese_convert"] = "t2s" });
```

## Planner improvements and runtime settings

DISTINCT, aggregates over joins, date grouping, and range ordering use normal ORM query expressions. ParadeDB chooses eligible pushdowns automatically. Runtime settings, including spill behavior and vector scan limits, can be configured through the framework's normal SQL connection API and require no separate query helpers.

## Search tokenizer and index tuning

`search_tokenizer` sets the default search-time tokenizer. `layer_sizes` and `background_layer_sizes` are comma-separated PostgreSQL size strings, such as `"100MB, 1GB"`. Use `"0"` to disable the corresponding merge mode. `mutable_segment_rows` accepts an integer from 0 through 10000; 0 disables mutable segments. Omitted options keep the server defaults.

```csharp
entity.HasParadeDbIndex("items_search_idx", item => item.Id)
    .HasField(item => item.Description)
    .HasSearchTokenizer(Tokenizer.Simple(new() { ["lowercase"] = false }))
    .HasLayerSizes("0")
    .HasBackgroundLayerSizes("100MB, 1GB")
    .HasMutableSegmentRows(1000);

var boolean = SearchQuery.Boolean(
    should: [SearchQuery.Parse("Description:running"), SearchQuery.Parse("Description:shoes")],
    minimumShouldMatch: 2,
    mustNot: [SearchQuery.Parse("Category:discontinued")]);
var query = SearchQuery.DisjunctionMax([boolean, SearchQuery.Parse("Description:boots")], tieBreaker: 0.5f);
var rows = await context.Items.Where(item => EF.Functions.Search(item.Id, query)).ToListAsync();
var result = await context.Database.Aggregate(
    "items_search_idx", query,
    new { count = new { value_count = new { field = "Id" } } },
    new() { MemoryLimit = 10000000, BucketLimit = 100, Visibility = "transaction" }
).SingleAsync();
```

`SnippetOptions` and `SnippetPositionsOptions` expose `Limit` and `Offset`. Pass snippet options and `SearchQuery` objects as query constants, as with tokenizer options. Index annotations survive EF migration generation.

Query strings use ParadeDB's query syntax and may include field qualifiers. Boolean clauses and disjunction-max inputs can be nested. `minimum_should_match` is a non-negative integer. `tie_breaker` ranges from 0 to 1: 0 uses the best matching subquery's score, while larger values also include contributions from other matching subqueries. Apply composed queries to the index's first key column, and refer to actual index field names or aliases in field qualifiers.

## Highlight pagination and direct aggregate limits

ParadeDB 0.26.0 accepts `limit` and `offset` on `snippet`, but deprecates them in favor of `snippets`. Prefer the existing multiple-snippet helper for new paginated highlighting. `snippet_positions` pagination is also exposed. Formatting and pagination options are optional; omitting earlier options retains the server defaults.

The direct aggregate helper calls `paradedb.aggregate`, independently of the existing `pdb.agg` expression. It accepts positive integer memory and bucket limits. The memory limit is in bytes and defaults to 500000000; an omitted bucket limit uses the server setting. Visibility accepts `transaction`, `raw`, or `threshold`. Supply either visibility or the compatibility `solve_mvcc` boolean, never both. Database errors, including exceeding a resource limit, propagate to the caller.
