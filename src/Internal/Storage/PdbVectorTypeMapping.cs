using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace ParadeDB.EntityFrameworkCore.Internal.Storage;

internal sealed class PdbVectorTypeMapping : RelationalTypeMapping
{
    public static readonly PdbVectorTypeMapping Default = new("vector");

    public PdbVectorTypeMapping(string storeType)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(float[]),
                    converter: null,
                    comparer: new ValueComparer<float[]>(
                        (a, b) => a == null ? b == null : b != null && a.SequenceEqual(b),
                        v => v.Aggregate(17, (hash, value) => HashCode.Combine(hash, value)),
                        v => v.ToArray()
                    )
                ),
                storeType
            )
        ) { }

    private PdbVectorTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters) =>
        new PdbVectorTypeMapping(parameters);

    protected override void ConfigureParameter(DbParameter parameter) =>
        ((NpgsqlParameter)parameter).DataTypeName = "vector";

    protected override string GenerateNonNullSqlLiteral(object value) =>
        $"'[{string.Join(",", ((float[])value).Select(v => v.ToString(CultureInfo.InvariantCulture)))}]'";
}
