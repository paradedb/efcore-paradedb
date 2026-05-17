# efcore-paradedb

## Verifying your changes

Make sure to run the following to verify your changes:

```bash
dotnet tool restore
dotnet csharpier check .
dotnet build build.slnf --configuration Release
dotnet test tests/ParadeDB.EntityFrameworkCore.Tests/ParadeDB.EntityFrameworkCore.Tests.csproj --configuration Release
dotnet test tests/ParadeDB.EntityFrameworkCore.IntegrationTests/ParadeDB.EntityFrameworkCore.IntegrationTests.csproj --configuration Release
```

All must pass with 0 failures.

## Before Committing

Always verify your changes before committing.

## Changelog

When you make a change that a user of this project would care about, record it in the `Unreleased` section of the changelog. If the change is breaking, make sure to denote that.
