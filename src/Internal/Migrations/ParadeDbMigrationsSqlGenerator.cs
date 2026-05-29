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
            operation.FindAnnotation(ParadeDbAnnotationNames.Bm25Fields)?.Value
                is not string[] fields
            || operation.FindAnnotation(ParadeDbAnnotationNames.Bm25KeyField)?.Value
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
            operation.FindAnnotation(ParadeDbAnnotationNames.Bm25SearchTokenizer)?.Value as string;

        builder
            .Append("CREATE INDEX ")
            .Append(createdConcurrently ? "CONCURRENTLY " : "")
            .Append(helper.DelimitIdentifier(operation.Name))
            .Append(" ON ")
            .Append(helper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" USING bm25 (")
            .Append(string.Join(", ", fields))
            .Append(") WITH (key_field = ")
            .Append(stringMapping.GenerateSqlLiteral(keyField));

        if (searchTokenizer is not null)
        {
            builder
                .Append(", search_tokenizer = ")
                .Append(stringMapping.GenerateSqlLiteral(searchTokenizer));
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
