#!/usr/bin/env bash
#
# run_tests.sh
#
# Runs the full test suite. The tests provision their own database with
# Testcontainers, so no ParadeDB container is started here. Extra arguments are
# forwarded to dotnet test.

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${ROOT_DIR}"

dotnet test --framework "${DOTNET_FRAMEWORK:-net10.0}" "$@"
