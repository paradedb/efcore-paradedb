#!/usr/bin/env bash
#
# smoke_package_install.sh
#
# Packs the NuGet package, restores it into a throwaway project from a local
# feed, and compiles against the public API. This catches packaging problems the
# test suite cannot see, because the tests reference the project directly while
# consumers get only what the .nupkg actually ships.

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${ROOT_DIR}"

PROJECT="src/ParadeDB.EntityFrameworkCore.csproj"
PACKAGE_ID="ParadeDB.EntityFrameworkCore"

WORK_DIR="$(mktemp -d "${TMPDIR:-/tmp}/efcore-paradedb-smoke.XXXXXX")"
cleanup() {
  rm -rf "${WORK_DIR}"
}
trap cleanup EXIT

FEED_DIR="${WORK_DIR}/feed"
APP_DIR="${WORK_DIR}/app"

echo "Packing ${PACKAGE_ID}..."
dotnet pack "${PROJECT}" --configuration Release --output "${FEED_DIR}" >/dev/null

NUPKG="$(find "${FEED_DIR}" -maxdepth 1 -name "${PACKAGE_ID}.*.nupkg" ! -name '*.symbols.nupkg' | head -1)"
if [[ -z "${NUPKG}" ]]; then
  echo "❌ dotnet pack produced no .nupkg" >&2
  exit 1
fi

VERSION="$(basename "${NUPKG}")"
VERSION="${VERSION#"${PACKAGE_ID}."}"
VERSION="${VERSION%.nupkg}"

mkdir -p "${APP_DIR}"
cd "${APP_DIR}"

dotnet new console --output . >/dev/null

# Restore the packed package from the local feed only; its dependencies still
# come from nuget.org.
cat > nuget.config <<XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="paradedb-smoke" value="${FEED_DIR}" />
  </packageSources>
</configuration>
XML

cat > Program.cs <<'CSHARP'
using System.Reflection;
using ParadeDB.EntityFrameworkCore.Extensions;

// Referencing these at compile time proves the public API shipped in the
// package; reflecting over them proves the assembly loads at runtime.
var type = typeof(ParadeDbFunctionsExtensions);
var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);

foreach (var name in new[] { "MatchAll", "Score", "Parse" })
{
    if (!methods.Any(m => m.Name == name))
    {
        Console.Error.WriteLine($"Expected public static {type.Name}.{name} in the packed assembly");
        return 1;
    }
}

Console.WriteLine($"Package smoke install passed for {type.Assembly.GetName().Name}");
return 0;
CSHARP

dotnet add package "${PACKAGE_ID}" --version "${VERSION}" >/dev/null
dotnet run --configuration Release

echo "✅ Package smoke install passed for ${PACKAGE_ID} ${VERSION}"
