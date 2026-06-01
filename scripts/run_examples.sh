#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${ROOT}"

source scripts/run_paradedb.sh
set -euo pipefail

examples=(
  Quickstart
  FacetedSearch
  Autocomplete
  MoreLikeThis
  HybridRrf
  Rag
)

for example in "${examples[@]}"; do
  echo
  echo "==> Running ${example}"
  dotnet run --project "examples/Examples.csproj" --framework net10.0 -p:Example="${example}"
done
