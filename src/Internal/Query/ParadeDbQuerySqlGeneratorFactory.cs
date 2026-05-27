using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace ParadeDB.EntityFrameworkCore.Internal.Query;

internal sealed class ParadeDbQuerySqlGeneratorFactory : IQuerySqlGeneratorFactory
{
    private readonly QuerySqlGeneratorDependencies _dependencies;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly INpgsqlSingletonOptions _npgsqlOptions;

    public ParadeDbQuerySqlGeneratorFactory(
        QuerySqlGeneratorDependencies dependencies,
        IRelationalTypeMappingSource typeMappingSource,
        INpgsqlSingletonOptions npgsqlOptions
    )
    {
        _dependencies = dependencies;
        _typeMappingSource = typeMappingSource;
        _npgsqlOptions = npgsqlOptions;
    }

    public QuerySqlGenerator Create() =>
        new ParadeDbQuerySqlGenerator(
            _dependencies,
            _typeMappingSource,
            _npgsqlOptions.ReverseNullOrderingEnabled,
            _npgsqlOptions.PostgresVersion
        );
}
