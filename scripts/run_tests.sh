#!/usr/bin/env bash
#
# run_tests.sh
#
# Runs the full test suite against PARADEDB_TEST_DSN when supplied, or a
# Testcontainers database using the requested ParadeDB release. Extra arguments
# are forwarded to dotnet test.

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${ROOT_DIR}"

export PARADEDB_IMAGE="${PARADEDB_IMAGE:-paradedb/paradedb:${PARADEDB_VERSION:-0.26.0}-pg${PARADEDB_POSTGRES_VERSION:-18}}"

dotnet test --framework "${DOTNET_FRAMEWORK:-net10.0}" "$@"
