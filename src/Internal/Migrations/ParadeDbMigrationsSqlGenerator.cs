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
            || operation.FindAnnotation(ParadeDbAnnotationNames.IndexKeyField)?.Value
                is not string keyField
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
            .Append(") WITH (key_field = ")
            .Append(stringMapping.GenerateSqlLiteral(keyField));

        if (searchTokenizer is not null)
        {
            builder
                .Append(", search_tokenizer = ")
                .Append(stringMapping.GenerateSqlLiteral(searchTokenizer));
        }

        if (
            operation.FindAnnotation(ParadeDbAnnotationNames.IndexCentroidRatio)?.Value
            is double centroidRatio
        )
        {
            builder
                .Append(", centroid_ratio = ")
                .Append(centroidRatio.ToString(CultureInfo.InvariantCulture));
        }

        if (
            operation.FindAnnotation(ParadeDbAnnotationNames.IndexTrainingSamplesPerCentroid)?.Value
            is int trainingSamplesPerCentroid
        )
        {
            builder
                .Append(", training_samples_per_centroid = ")
                .Append(trainingSamplesPerCentroid.ToString(CultureInfo.InvariantCulture));
        }

        if (
            operation.FindAnnotation(ParadeDbAnnotationNames.IndexClusterReplication)?.Value
            is int clusterReplication
        )
        {
            builder
                .Append(", cluster_replication = ")
                .Append(clusterReplication.ToString(CultureInfo.InvariantCulture));
        }

        builder.Append(")");

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
