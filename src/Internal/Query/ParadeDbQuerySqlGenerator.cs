using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using ParadeDB.EntityFrameworkCore.Internal.Query.Expressions;

namespace ParadeDB.EntityFrameworkCore.Internal.Query;

internal sealed class ParadeDbQuerySqlGenerator : NpgsqlQuerySqlGenerator
{
    public ParadeDbQuerySqlGenerator(
        QuerySqlGeneratorDependencies dependencies,
        IRelationalTypeMappingSource typeMappingSource,
        bool reverseNullOrderingEnabled,
        Version postgresVersion
    )
        : base(dependencies, typeMappingSource, reverseNullOrderingEnabled, postgresVersion) { }

    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression is PdbOverExpression overExpression)
        {
            base.VisitExtension(overExpression);
            Sql.Append(" OVER ()");
            return extensionExpression;
        }

        return base.VisitExtension(extensionExpression);
    }
}
