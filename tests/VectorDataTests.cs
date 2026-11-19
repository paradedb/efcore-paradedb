using System.Data.Common;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.VectorData;
using Npgsql;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Tests.Persistence;
using ParadeDB.VectorData;
using Shouldly;

namespace ParadeDB.EntityFrameworkCore.Tests;

public sealed class VectorDataTests : TestBase
{
    [Test]
    public async Task SearchAsync_FiltersBeforeRankingAndUsesParadeDbTopK()
    {
        var capture = new SearchCommandCapture();
        await using var context = DbFixture.CreateContext(capture);
        await using var transaction = await context.Database.BeginTransactionAsync();

        var firstId = await context.MockItems.MaxAsync(item => item.Id) + 1;
        float[] queryEmbedding = [1, 0, 0, 0, 0, 0, 0, 0];
        context.MockItems.AddRange(
            new MockItem
            {
                Id = firstId,
                Rating = 1,
                Embedding = queryEmbedding,
            },
            new MockItem
            {
                Id = firstId + 1,
                Rating = 5,
                Embedding = [3, 4, 0, 0, 0, 0, 0, 0],
            },
            new MockItem
            {
                Id = firstId + 2,
                Rating = 5,
                Embedding = [0, 1, 0, 0, 0, 0, 0, 0],
            },
            new MockItem
            {
                Id = firstId + 3,
                Rating = 5,
                Embedding = [-1, 0, 0, 0, 0, 0, 0, 0],
            },
            new MockItem { Id = firstId + 4, Rating = 5 }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        IVectorSearchable<MockItem> adapter = new ParadeDbVectorSearch<MockItem>(
            context.MockItems.Where(item => EF.Functions.All(item.Id)),
            nameof(MockItem.Embedding),
            item => new MockItem { Id = item.Id, Rating = item.Rating }
        );

        var matches = new List<VectorSearchResult<MockItem>>();
        await foreach (
            var match in adapter.SearchAsync(
                (ReadOnlyMemory<float>)queryEmbedding,
                2,
                new() { Filter = item => item.Id >= firstId && item.Rating == 5 }
            )
        )
            matches.Add(match);

        matches.Select(match => match.Record.Id).ShouldBe([firstId + 1, firstId + 2]);
        matches[0].Score!.Value.ShouldBe(0.6, 0.00001);
        matches[1].Score!.Value.ShouldBe(0, 0.00001);
        matches.ShouldAllBe(match => match.Record.Embedding == null);
        context.ChangeTracker.Entries().ShouldBeEmpty();

        await using var explain = capture.Command.ShouldNotBeNull();
        const string expectedSql = """
            SELECT m."Id", m."Rating", 1.0 - (m."Embedding" <=> @embedding)
            FROM "MockItems" AS m
            WHERE (m."Id" @@@ pdb.all()) AND m."Id" >= @firstId AND m."Rating" = 5 AND m."Embedding" IS NOT NULL
            ORDER BY m."Embedding" <=> @embedding
            LIMIT @p
            """;
        Regex
            .Replace(explain.CommandText.Trim().ReplaceLineEndings("\n"), @"@__(\w+?)_\d+", "@$1")
            .ShouldBe(expectedSql.ReplaceLineEndings("\n"));
        explain.CommandText = "EXPLAIN " + explain.CommandText;
        explain.Transaction = (NpgsqlTransaction)transaction.GetDbTransaction();
        await using (var reader = await explain.ExecuteReaderAsync())
        {
            var plan = new List<string>();
            while (await reader.ReadAsync())
                plan.Add(reader.GetString(0));
            string.Join(Environment.NewLine, plan).ShouldContain("TopKScanExecState");
        }

        // A query without a ParadeDB predicate can use PostgreSQL's ordinary SQL execution.
        var fallback = new ParadeDbVectorSearch<MockItem>(
            context.MockItems,
            nameof(MockItem.Embedding),
            item => new MockItem { Id = item.Id }
        );
        matches.Clear();
        await foreach (
            var match in fallback.SearchAsync(
                queryEmbedding,
                5,
                new()
                {
                    Filter = item => item.Id == firstId || item.Id == firstId + 4,
                    IncludeVectors = true,
                }
            )
        )
            matches.Add(match);

        var matchWithVector = matches.ShouldHaveSingleItem();
        matchWithVector.Record.Id.ShouldBe(firstId);
        matchWithVector.Record.Embedding.ShouldBe(queryEmbedding);
    }

    private sealed class SearchCommandCapture : DbCommandInterceptor
    {
        public NpgsqlCommand? Command { get; private set; }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default
        )
        {
            if (Command is null && command.CommandText.Contains("<=>", StringComparison.Ordinal))
                Command = ((NpgsqlCommand)command).Clone();

            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
