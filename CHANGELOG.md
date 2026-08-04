# Changelog

All notable changes to this project will be documented in this file. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Vector index build options on ParadeDB indexes: `HasCentroidRatio`,
  `HasTrainingSamplesPerCentroid`, and `HasClusterReplication` emit the
  `centroid_ratio`, `training_samples_per_centroid`, and `cluster_replication`
  `WITH` options in index DDL.

## [0.2.0] - 2026-08-04

### Added

- Native vector search support: `vector(n)` column mapping for `float[]`
  properties, vector fields with metric operator classes in
  ParadeDB indexes (`HasField(..., VectorMetric)`), and the `L2Distance`,
  `CosineDistance`, and `InnerProduct` distance functions.

### Changed

- **Breaking**: `HasBm25Index` and `Bm25IndexBuilder<TEntity>` are renamed to
  `HasParadeDbIndex` and `ParadeDbIndexBuilder<TEntity>`, the `ParadeDB:Bm25*`
  model annotations are renamed to `ParadeDB:Index*`, and index DDL now always
  emits `USING paradedb`. This release requires pg_search 0.25.0+.

## Fixed

- Correctly quote upper case column names in index creation statements.

## [0.1.2] - 2026-07-14

## Changed

- Updated documentation and copy

## [0.1.1] - 2026-06-01

## Added

- Added a dedicated README that can be properly rendered on NuGet.

## [0.1.0] - 2026-06-01

### Added

- Support for the ParadeDB query language, index management, and diagnostics.

[0.2.0]: https://github.com/paradedb/efcore-paradedb/compare/v0.1.2...v0.2.0
[0.1.2]: https://github.com/paradedb/efcore-paradedb/compare/v0.1.1...v0.1.2
[0.1.1]: https://github.com/paradedb/efcore-paradedb/compare/v0.1.0...v0.1.1
[0.1.0]: https://github.com/paradedb/efcore-paradedb/compare/v0.1.0
