using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using ParadeDB.EntityFrameworkCore;
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Internal.Query.Expressions;
using ParadeDB.EntityFrameworkCore.Internal.Storage;
using ParadeDB.EntityFrameworkCore.Modifiers;

internal sealed class Translator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _jsonbTypeMapping;

    public Translator(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource
    )
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _jsonbTypeMapping = typeMappingSource.FindMapping(typeof(JsonElement), "jsonb")!;
    }

    public SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger
    )
    {
        return method.Name switch
        {
            nameof(Pdb.Boost) or nameof(Pdb.Const) or nameof(Pdb.Fuzzy) or nameof(Pdb.Slop) =>
                BuildModifier(arguments, method.Name),
            nameof(ParadeDbFunctionsExtensions.MatchAny) => BuildOperator(
                arguments,
                PdbOperatorType.Disjunction
            ),
            nameof(ParadeDbFunctionsExtensions.MatchAll) => BuildOperator(
                arguments,
                PdbOperatorType.Conjunction
            ),
            nameof(ParadeDbFunctionsExtensions.Phrase) => BuildOperator(
                arguments,
                PdbOperatorType.Phrase
            ),
            nameof(ParadeDbFunctionsExtensions.Term) => BuildOperator(
                arguments,
                PdbOperatorType.Term
            ),
            nameof(ParadeDbFunctionsExtensions.Score) => _sqlExpressionFactory.Function(
                name: "score",
                schema: "pdb",
                nullable: false,
                arguments: [arguments[1]],
                argumentsPropagateNullability: [false],
                returnType: typeof(float)
            ),
            nameof(Pdb.All) => _sqlExpressionFactory.Function(
                name: "all",
                schema: "pdb",
                nullable: false,
                arguments: [],
                argumentsPropagateNullability: [],
                returnType: typeof(bool)
            ),
            nameof(ParadeDbFunctionsExtensions.Snippet) => BuildSnippet(arguments),
            nameof(ParadeDbFunctionsExtensions.Query)
                when method.DeclaringType == typeof(ParadeDbFunctionsExtensions) =>
                BuildQueryBuilder(arguments),
            nameof(Pdb.Proximity) => _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]),
            nameof(Pdb.ProximityRegex) => BuildProximityFunction("prox_regex", arguments),
            nameof(Pdb.ProximityArray) => BuildProximityFunction("prox_array", arguments),
            nameof(ParadeDbFunctionsExtensions.Tokenize) => BuildTokenizer(arguments, false),
            nameof(ParadeDbFunctionsExtensions.TokenizeAsArray) => BuildTokenizer(arguments, true),
            nameof(PdbProximityQueryExtensions.Within) => BuildProximityExpression(arguments),
            _ when method.DeclaringType == typeof(PdbMoreLikeThisQueryExtensions) =>
                BuildtMoreLikeThisOption(method, arguments),
            nameof(Pdb.Document) => BuildMoreLikeThisOptionBuilder(arguments),
            nameof(Pdb.DocumentId) => BuildMoreLikeThisOptionBuilder(arguments),
            nameof(ParadeDbFunctionsExtensions.MoreLikeThis) => BuildMoreLikeThis(arguments),
            nameof(ParadeDbFunctionsExtensions.Alias) => BuildAlias(arguments),
            nameof(ParadeDbFunctionsExtensions.Agg) => BuildAggregate(arguments, false),
            nameof(ParadeDbFunctionsExtensions.AggOver) => BuildAggregate(arguments, true),
            _ => null,
        };
    }

    private SqlExpression? BuildModifier(IReadOnlyList<SqlExpression> arguments, string methodName)
    {
        RelationalTypeMapping typeMapping = methodName switch
        {
            nameof(Pdb.Boost) when arguments[1] is SqlConstantExpression { Value: float factor } =>
                new PdbModifierTypeMapping<Boost>(new Boost(factor)),
            nameof(Pdb.Const) when arguments[1] is SqlConstantExpression { Value: float score } =>
                new PdbModifierTypeMapping<Const>(new Const(score)),
            nameof(Pdb.Fuzzy) when arguments[1] is SqlConstantExpression { Value: int distance } =>
                new PdbModifierTypeMapping<Fuzzy>(
                    new Fuzzy(
                        distance,
                        arguments.Count > 2
                            && arguments[2] is SqlConstantExpression { Value: true },
                        arguments.Count > 3 && arguments[3] is SqlConstantExpression { Value: true }
                    )
                ),
            nameof(Pdb.Slop) when arguments[1] is SqlConstantExpression { Value: int slop } =>
                new PdbModifierTypeMapping<Slop>(new Slop(slop)),
            _ => throw new InvalidOperationException("Unexpected modifier type"),
        };

        return _sqlExpressionFactory.MakeUnary(
            ExpressionType.Convert,
            _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]),
            typeMapping.ClrType,
            typeMapping
        );
    }

    private SqlExpression? BuildOperator(
        IReadOnlyList<SqlExpression> arguments,
        PdbOperatorType operatorType
    )
    {
        var left = arguments[1];
        var right = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]);

        return new PdbBoolExpression(left, right, operatorType);
    }

    private SqlExpression? BuildSnippet(IReadOnlyList<SqlExpression> arguments)
    {
        List<SqlExpression> args = [arguments[1]];
        List<bool> argsNullability = [false];

        if (arguments.Count == 3)
        {
            args.AddRange([
                _sqlExpressionFactory.Constant("<b>"),
                _sqlExpressionFactory.Constant("</b>"),
                arguments[2],
            ]);

            argsNullability.AddRange([false, false, false]);
        }
        else if (arguments.Count == 4)
        {
            args.AddRange([arguments[2], arguments[3]]);
            argsNullability.AddRange([false, false]);
        }
        else if (arguments.Count == 5)
        {
            args.AddRange([arguments[2], arguments[3], arguments[4]]);
            argsNullability.AddRange([false, false, false]);
        }

        return _sqlExpressionFactory.Function(
            name: "snippet",
            schema: "pdb",
            nullable: true,
            arguments: args,
            argumentsPropagateNullability: argsNullability,
            returnType: typeof(string)
        );
    }

    private SqlExpression? BuildQueryBuilder(IReadOnlyList<SqlExpression> arguments)
    {
        var column = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]);
        var query = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]);
        return new PdbBoolExpression(column, query, PdbOperatorType.Function);
    }

    private PdbProximityExpression BuildProximityExpression(IReadOnlyList<SqlExpression> arguments)
    {
        var left = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]);
        var distance = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]);
        var right = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]);
        var ordered = arguments[3] is SqlConstantExpression { Value: true };
        var withDistance = new PdbProximityExpression(left, distance, ordered);
        return new PdbProximityExpression(withDistance, right, ordered);
    }

    private SqlExpression BuildProximityFunction(
        string name,
        IReadOnlyList<SqlExpression> arguments
    )
    {
        var flattened = FlattenProximityArguments(arguments);
        var mapped = flattened
            .Select(a => _sqlExpressionFactory.ApplyDefaultTypeMapping(a))
            .ToArray();

        return _sqlExpressionFactory.Function(
            schema: "pdb",
            name: name,
            arguments: mapped,
            nullable: true,
            argumentsPropagateNullability: new bool[mapped.Length],
            returnType: typeof(PdbProximityQuery),
            typeMapping: PdbTypeMappings.Text
        );
    }

    private IReadOnlyList<SqlExpression> FlattenProximityArguments(
        IReadOnlyList<SqlExpression> arguments
    )
    {
        if (arguments.Count != 1)
        {
            return arguments;
        }

        var arg = arguments[0];

        if (arg is PgNewArrayExpression pgArray)
        {
            return pgArray.Expressions;
        }

        if (arg is SqlConstantExpression { Value: IEnumerable enumerable and not string })
        {
            var result = new List<SqlExpression>();

            foreach (var item in enumerable)
            {
                result.Add(_sqlExpressionFactory.Constant(item));
            }

            return result;
        }

        return arguments;
    }

    private SqlExpression? BuildTokenizer(IReadOnlyList<SqlExpression> arguments, bool asArray)
    {
        if (arguments[2] is not SqlConstantExpression { Value: Tokenizer tokenizer })
        {
            return null;
        }

        var typeMapping = new TokenizerTypeMapping(tokenizer, asArray);

        return _sqlExpressionFactory.MakeUnary(
            ExpressionType.Convert,
            arguments[1],
            typeMapping.ClrType,
            typeMapping
        );
    }

    private PdbAccumulatorExpression BuildMoreLikeThisOptionBuilder(
        IReadOnlyList<SqlExpression> arguments
    )
    {
        var seed = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]);
        return new PdbAccumulatorExpression([seed], [null]);
    }

    private PdbAccumulatorExpression? BuildtMoreLikeThisOption(
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments
    )
    {
        if (arguments[0] is not PdbAccumulatorExpression carrier)
        {
            return null;
        }

        var value = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]);

        return method.Name switch
        {
            nameof(PdbMoreLikeThisQueryExtensions.Fields) => carrier.AppendPositional(value),
            nameof(PdbMoreLikeThisQueryExtensions.MinTermFrequency) => carrier.AppendNamed(
                "min_term_frequency",
                value
            ),
            nameof(PdbMoreLikeThisQueryExtensions.MinDocFrequency) => carrier.AppendNamed(
                "min_doc_frequency",
                value
            ),
            nameof(PdbMoreLikeThisQueryExtensions.MaxDocFrequency) => carrier.AppendNamed(
                "max_doc_frequency",
                value
            ),
            nameof(PdbMoreLikeThisQueryExtensions.MaxQueryTerms) => carrier.AppendNamed(
                "max_query_terms",
                value
            ),
            nameof(PdbMoreLikeThisQueryExtensions.MinWordLength) => carrier.AppendNamed(
                "min_word_length",
                value
            ),
            nameof(PdbMoreLikeThisQueryExtensions.MaxWordLength) => carrier.AppendNamed(
                "max_word_length",
                value
            ),
            nameof(PdbMoreLikeThisQueryExtensions.Stopwords) => carrier.AppendNamed(
                "stopwords",
                value
            ),
            _ => null,
        };
    }

    private PdbBoolExpression? BuildMoreLikeThis(IReadOnlyList<SqlExpression> arguments)
    {
        if (arguments[2] is not PdbAccumulatorExpression carrier)
        {
            return null;
        }

        var column = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]);

        var function = PgFunctionExpression.CreateWithNamedArguments(
            name: "pdb.more_like_this",
            arguments: carrier.Arguments,
            argumentNames: carrier.ArgumentNames,
            nullable: false,
            argumentsPropagateNullability: new bool[carrier.Arguments.Count],
            builtIn: false,
            type: typeof(PdbMoreLikeThisQuery),
            typeMapping: PdbTypeMappings.Text
        );

        return new PdbBoolExpression(column, function, PdbOperatorType.Function);
    }

    private SqlExpression? BuildAlias(IReadOnlyList<SqlExpression> arguments)
    {
        if (arguments[2] is not SqlConstantExpression { Value: string alias })
        {
            throw new InvalidOperationException("A pdb alias must have an alias value");
        }

        var typeMapping = new AliasTypeMapping(alias);

        return _sqlExpressionFactory.MakeUnary(
            ExpressionType.Convert,
            arguments[1],
            typeMapping.ClrType,
            typeMapping
        );
    }

    private SqlExpression? BuildAggregate(IReadOnlyList<SqlExpression> arguments, bool over)
    {
        if (arguments[1] is not SqlConstantExpression { Value: { } aggregate })
        {
            return null;
        }

        List<SqlExpression> args =
        [
            _sqlExpressionFactory.Constant(JsonSerializer.Serialize(aggregate)),
            arguments[2],
        ];

        var function = _sqlExpressionFactory.Function(
            name: "agg",
            schema: "pdb",
            nullable: true,
            arguments: args,
            argumentsPropagateNullability: [false, false],
            returnType: typeof(JsonElement),
            typeMapping: _jsonbTypeMapping
        );

        return over ? new PdbOverExpression((SqlFunctionExpression)function) : function;
    }
}
