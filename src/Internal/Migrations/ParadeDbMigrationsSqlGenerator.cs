using System.Globalization;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using ParadeDB.EntityFrameworkCore.Internal.Metadata;

namespace ParadeDB.EntityFrameworkCore.Internal.Migrations;

internal sealed class ParadeDbMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
{
    public ParadeDbMigrationsSqlGenerator(
        MigrationsSqlGeneratorDependencies dependencies,
        INpgsqlSingletonOptions npgsqlSingletonOptions
    )
        : base(dependencies, npgsqlSingletonOptions) { }

    protected override void Generate(
        CreateIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true
    )
    {
        if (
            operation.FindAnnotation(ParadeDbAnnotationNames.IndexFields)?.Value
            is not string[] fields
        )
        {
            base.Generate(operation, model, builder, terminate);
            return;
        }

        var helper = Dependencies.SqlGenerationHelper;
        var stringMapping = Dependencies.TypeMappingSource.FindMapping(typeof(string))!;
        var createdConcurrently =
            operation.FindAnnotation(NpgsqlAnnotationNames.CreatedConcurrently)?.Value is true;
        var searchTokenizer =
            operation.FindAnnotation(ParadeDbAnnotationNames.IndexSearchTokenizer)?.Value as string;

        builder
            .Append("CREATE INDEX ")
            .Append(createdConcurrently ? "CONCURRENTLY " : "")
            .Append(helper.DelimitIdentifier(operation.Name))
            .Append(" ON ")
            .Append(helper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" USING paradedb (")
            .Append(string.Join(", ", fields))
            .Append(")");

        var options = new List<string>();
        if (
            operation.FindAnnotation(ParadeDbAnnotationNames.IndexKeyField)?.Value
            is string keyField
        )
        {
            options.Add($"key_field = {stringMapping.GenerateSqlLiteral(keyField)}");
        }

        if (searchTokenizer is not null)
        {
            options.Add($"search_tokenizer = {stringMapping.GenerateSqlLiteral(searchTokenizer)}");
        }

        if (
            operation.FindAnnotation(ParadeDbAnnotationNames.IndexCentroidRatio)?.Value
            is double centroidRatio
        )
        {
            options.Add($"centroid_ratio = {centroidRatio.ToString(CultureInfo.InvariantCulture)}");
        }

        if (
            operation.FindAnnotation(ParadeDbAnnotationNames.IndexTrainingSamplesPerCentroid)?.Value
            is int trainingSamplesPerCentroid
        )
        {
            options.Add(
                $"training_samples_per_centroid = {trainingSamplesPerCentroid.ToString(CultureInfo.InvariantCulture)}"
            );
        }

        if (
            operation.FindAnnotation(ParadeDbAnnotationNames.IndexClusterReplication)?.Value
            is int clusterReplication
        )
        {
            options.Add(
                $"cluster_replication = {clusterReplication.ToString(CultureInfo.InvariantCulture)}"
            );
        }

        if (options.Count > 0)
        {
            builder.Append(" WITH (").Append(string.Join(", ", options)).Append(")");
        }

        if (operation.Filter is not null)
        {
            builder.Append(" WHERE ").Append(operation.Filter);
        }

        if (terminate)
        {
            builder
                .AppendLine(helper.StatementTerminator)
                .EndCommand(suppressTransaction: createdConcurrently);
        }
    }
}
