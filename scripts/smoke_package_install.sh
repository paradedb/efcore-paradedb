#!/usr/bin/env bash
#
# smoke_package_install.sh
#
# Packs the NuGet packages, restores them into a throwaway project from a local
# feed, and compiles against the public API. This catches packaging problems the
# test suite cannot see, because the tests reference the project directly while
# consumers get only what the .nupkg actually ships.

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${ROOT_DIR}"

PROJECT="src/ParadeDB.EntityFrameworkCore.csproj"
PACKAGE_ID="ParadeDB.EntityFrameworkCore"
VECTOR_PACKAGE_ID="ParadeDB.VectorData"

WORK_DIR="$(mktemp -d "${TMPDIR:-/tmp}/efcore-paradedb-smoke.XXXXXX")"
cleanup() {
  rm -rf "${WORK_DIR}"
}
trap cleanup EXIT

FEED_DIR="${WORK_DIR}/feed"
APP_DIR="${WORK_DIR}/app"

VERSION="0.0.0-smoke"
for project in src/ParadeDB.EntityFrameworkCore.csproj src/VectorData/ParadeDB.VectorData.csproj; do
  echo "Packing ${project}..."
  dotnet pack "${project}" --configuration Release \
    --output "${FEED_DIR}" -p:PackageVersion="${VERSION}" --verbosity quiet
done

mkdir -p "${APP_DIR}"
cd "${APP_DIR}"

dotnet new console --output . >/dev/null

# Restore the packed package from the local feed only; its dependencies still
# come from nuget.org.
cat >nuget.config <<XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="paradedb-smoke" value="${FEED_DIR}" />
  </packageSources>
</configuration>
XML

cat >Program.cs <<'CSHARP'
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

if grep -Fq 'Microsoft.Extensions.VectorData.Abstractions/' obj/project.assets.json; then
  echo "VectorData must be opt-in for EF Core consumers" >&2
  exit 1
fi

cat >Program.cs <<'CSHARP'
using Microsoft.Extensions.VectorData;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.VectorData;

if (!typeof(IVectorSearchable<object>).IsAssignableFrom(typeof(ParadeDbVectorSearch<object>)))
{
    Console.Error.WriteLine("Expected the packed adapter to implement IVectorSearchable");
    return 1;
}

Console.WriteLine($"Loaded {typeof(ParadeDbVectorSearch<object>).Assembly.GetName().Name} over {typeof(ParadeDbFunctionsExtensions).Assembly.GetName().Name}");
return 0;
CSHARP

dotnet remove package "${PACKAGE_ID}" >/dev/null
dotnet add package "${VECTOR_PACKAGE_ID}" --version "${VERSION}" >/dev/null
dotnet run --configuration Release

echo "✅ Package smoke install passed for ${VECTOR_PACKAGE_ID} ${VERSION}"
