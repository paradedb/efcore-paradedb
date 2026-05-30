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
using ParadeDB.EntityFrameworkCore.Extensions;
using ParadeDB.EntityFrameworkCore.Internal.Query.Expressions;
using ParadeDB.EntityFrameworkCore.Internal.Storage;
using ParadeDB.EntityFrameworkCore.Modifiers;

namespace ParadeDB.EntityFrameworkCore.Internal.Query;

internal sealed class Translator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    public Translator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
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
            nameof(ParadeDbFunctionsExtensions.All) => BuildQueryBuilderFunction(
                arguments[1],
                _sqlExpressionFactory.Function(
                    name: "pdb.all",
                    nullable: false,
                    arguments: [],
                    argumentsPropagateNullability: [],
                    returnType: typeof(bool)
                )
            ),
            nameof(ParadeDbFunctionsExtensions.Exists) => BuildQueryBuilderFunction(
                arguments[1],
                _sqlExpressionFactory.Function(
                    name: "pdb.exists",
                    nullable: false,
                    arguments: [],
                    argumentsPropagateNullability: [],
                    returnType: typeof(bool)
                )
            ),
            nameof(ParadeDbFunctionsExtensions.Parse) => BuildQueryBuilderFunction(
                arguments[1],
                BuildParse(arguments)
            ),
            nameof(ParadeDbFunctionsExtensions.Regex) => BuildQueryBuilderFunction(
                arguments[1],
                _sqlExpressionFactory.Function(
                    name: "pdb.regex",
                    nullable: false,
                    arguments: [arguments[2]],
                    argumentsPropagateNullability: [false],
                    returnType: typeof(bool)
                )
            ),
            nameof(ParadeDbFunctionsExtensions.RegexPhrase) => BuildQueryBuilderFunction(
                arguments[1],
                BuildRegexPhrase(arguments)
            ),
            nameof(ParadeDbFunctionsExtensions.RangeTerm) => BuildQueryBuilderFunction(
                arguments[1],
                BuildRangeTerm(arguments)
            ),
            nameof(ParadeDbFunctionsExtensions.PhrasePrefix) => BuildQueryBuilderFunction(
                arguments[1],
                BuildPhrasePrefix(arguments)
            ),
            nameof(ParadeDbFunctionsExtensions.Snippet) => BuildSnippet(arguments),
            nameof(ParadeDbFunctionsExtensions.Snippets) => BuildSnippets(arguments),
            nameof(ParadeDbFunctionsExtensions.SnippetPositions) => _sqlExpressionFactory.Function(
                name: "pdb.snippet_positions",
                nullable: true,
                arguments: [arguments[1]],
                argumentsPropagateNullability: [false],
                returnType: typeof(int[,])
            ),
            nameof(ParadeDbFunctionsExtensions.Proximity)
                when method.DeclaringType == typeof(ParadeDbFunctionsExtensions) =>
                BuildQueryBuilderFunction(arguments[1], arguments[2]),
            nameof(Pdb.Proximity) => _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0]),
            nameof(Pdb.ProximityRegex) => BuildProximityFunction("prox_regex", arguments),
            nameof(Pdb.ProximityArray) => BuildProximityFunction("prox_array", arguments),
            nameof(ParadeDbFunctionsExtensions.Tokenize) => BuildTokenizer(arguments, false),
            nameof(ParadeDbFunctionsExtensions.TokenizeAsArray) => BuildTokenizer(arguments, true),
            nameof(PdbProximityQueryExtensions.Within) => BuildProximityExpression(arguments),
            _ when method.DeclaringType == typeof(PdbMoreLikeThisQueryExtensions) =>
                BuildMoreLikeThisOption(method, arguments),
            nameof(Pdb.Document) or nameof(Pdb.DocumentId) => BuildMoreLikeThisOptionBuilder(
                arguments
            ),
            nameof(ParadeDbFunctionsExtensions.MoreLikeThis) => BuildMoreLikeThis(arguments),
            nameof(ParadeDbFunctionsExtensions.Alias) => BuildAlias(arguments),
            nameof(ParadeDbFunctionsExtensions.Agg) => BuildAggregate(arguments, false),
            nameof(ParadeDbFunctionsExtensions.AggFilter) => BuildAggregate(
                arguments,
                false,
                arguments[2],
                3
            ),
            nameof(ParadeDbFunctionsExtensions.AggOver) => BuildAggregate(arguments, true),
            nameof(ParadeDbFunctionsExtensions.AggFilterOver) => BuildAggregate(
                arguments,
                true,
                arguments[2],
                3
            ),
            _ => null,
        };
    }

    private PgFunctionExpression BuildParse(IReadOnlyList<SqlExpression> arguments)
    {
        List<SqlExpression> args = [_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2])];
        List<string?> argNames = [null];

        Add(3, "lenient");
        Add(4, "conjunction_mode");

        return PgFunctionExpression.CreateWithNamedArguments(
            name: "pdb.parse",
            arguments: args,
            argumentNames: argNames,
            nullable: false,
            argumentsPropagateNullability: new bool[args.Count],
            builtIn: false,
            type: typeof(bool),
            typeMapping: null
        );

        void Add(int index, string name)
        {
            if (
                arguments.Count <= index
                || arguments[index] is SqlConstantExpression { Value: null }
            )
            {
                return;
            }

            args.Add(_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[index]));
            argNames.Add(name);
        }
    }

    private SqlExpression BuildPhrasePrefix(IReadOnlyList<SqlExpression> arguments)
    {
        List<SqlExpression> args = [arguments[2]];

        // If the user didn't pass in max_expansions, let the DB do the defaulting
        if (arguments[3] is not SqlConstantExpression { Value: null })
        {
            args.Add(arguments[3]);
        }

        return _sqlExpressionFactory.Function(
            name: "pdb.phrase_prefix",
            nullable: false,
            arguments: args,
            argumentsPropagateNullability: new bool[args.Count],
            returnType: typeof(bool)
        );
    }

    private PgFunctionExpression BuildRegexPhrase(IReadOnlyList<SqlExpression> arguments)
    {
        List<SqlExpression> args = [_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2])];
        List<string?> argNames = [null];

        if (arguments[3] is not SqlConstantExpression { Value: null })
        {
            args.Add(_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[3]));
            argNames.Add("slop");
        }

        if (arguments[4] is not SqlConstantExpression { Value: null })
        {
            args.Add(_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[4]));
            argNames.Add("max_expansions");
        }

        return PgFunctionExpression.CreateWithNamedArguments(
            name: "pdb.regex_phrase",
            arguments: args,
            argumentNames: argNames,
            nullable: false,
            argumentsPropagateNullability: new bool[args.Count],
            builtIn: false,
            type: typeof(bool),
            typeMapping: null
        );
    }

    private SqlExpression BuildRangeTerm(IReadOnlyList<SqlExpression> arguments)
    {
        List<SqlExpression> args = [_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2])];

        if (arguments.Count > 3)
        {
            args.Add(
                arguments[3] is SqlConstantExpression { Value: RangeTermRelation relation }
                    ? _sqlExpressionFactory.Constant(relation.ToString())
                    : arguments[3]
            );
        }

        return _sqlExpressionFactory.Function(
            name: "pdb.range_term",
            nullable: false,
            arguments: args,
            argumentsPropagateNullability: new bool[args.Count],
            returnType: typeof(bool)
        );
    }

    private SqlExpression BuildModifier(IReadOnlyList<SqlExpression> arguments, string methodName)
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

        if (arguments[0] is PdbBoolExpression boolExpression)
        {
            return new PdbBoolExpression(
                boolExpression.Left,
                ApplyModifier(boolExpression.Right),
                boolExpression.OperatorType
            );
        }

        return ApplyModifier(arguments[0]);

        SqlExpression ApplyModifier(SqlExpression expression) =>
            _sqlExpressionFactory.MakeUnary(
                ExpressionType.Convert,
                _sqlExpressionFactory.ApplyDefaultTypeMapping(expression),
                typeMapping.ClrType,
                typeMapping
            )!;
    }

    private PdbBoolExpression BuildOperator(
        IReadOnlyList<SqlExpression> arguments,
        PdbOperatorType operatorType
    )
    {
        var left = arguments[1];
        var right = _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]);

        return new PdbBoolExpression(left, right, operatorType);
    }

    private PgFunctionExpression BuildSnippet(IReadOnlyList<SqlExpression> arguments)
    {
        var options =
            arguments.Count == 3
                ? (SnippetOptions?)((SqlConstantExpression)arguments[2]).Value
                : null;
        List<SqlExpression> args = [arguments[1]];
        List<string?> argNames = [null];

        Add(options?.startTag, "start_tag");
        Add(options?.endTag, "end_tag");
        Add(options?.maxNumChars, "max_num_chars");

        return PgFunctionExpression.CreateWithNamedArguments(
            name: "pdb.snippet",
            arguments: args,
            argumentNames: argNames,
            nullable: true,
            argumentsPropagateNullability: new bool[args.Count],
            builtIn: false,
            type: typeof(string),
            typeMapping: null
        );

        void Add(object? value, string name)
        {
            if (value is null)
            {
                return;
            }

            args.Add(
                _sqlExpressionFactory.ApplyDefaultTypeMapping(_sqlExpressionFactory.Constant(value))
            );
            argNames.Add(name);
        }
    }

    private PgFunctionExpression BuildSnippets(IReadOnlyList<SqlExpression> arguments)
    {
        var options =
            arguments.Count == 3
                ? (SnippetsOptions?)((SqlConstantExpression)arguments[2]).Value
                : null;
        List<SqlExpression> args = [arguments[1]];
        List<string?> argNames = [null];

        Add(options?.startTag, "start_tag");
        Add(options?.endTag, "end_tag");
        Add(options?.maxNumChars, "max_num_chars");
        Add(options?.limit, "\"limit\"");
        Add(options?.offset, "\"offset\"");
        Add(options?.sortBy, "sort_by");

        return PgFunctionExpression.CreateWithNamedArguments(
            name: "pdb.snippets",
            arguments: args,
            argumentNames: argNames,
            nullable: true,
            argumentsPropagateNullability: new bool[args.Count],
            builtIn: false,
            type: typeof(string[]),
            typeMapping: PdbTypeMappings.TextArray
        );

        void Add(object? value, string name)
        {
            if (value is null)
            {
                return;
            }

            args.Add(
                _sqlExpressionFactory.ApplyDefaultTypeMapping(_sqlExpressionFactory.Constant(value))
            );
            argNames.Add(name);
        }
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

    private PdbAccumulatorExpression? BuildMoreLikeThisOption(
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

    private SqlExpression? BuildAggregate(
        IReadOnlyList<SqlExpression> arguments,
        bool over,
        SqlExpression? filter = null,
        int exactArgumentIndex = 2
    )
    {
        if (arguments[1] is not SqlConstantExpression { Value: { } aggregate })
        {
            return null;
        }

        List<SqlExpression> args =
        [
            _sqlExpressionFactory.Constant(JsonSerializer.Serialize(aggregate)),
            arguments[exactArgumentIndex],
        ];

        SqlExpression function = _sqlExpressionFactory.Function(
            name: "agg",
            schema: "pdb",
            nullable: true,
            arguments: args,
            argumentsPropagateNullability: [false, false],
            returnType: typeof(JsonElement),
            typeMapping: PdbTypeMappings.JsonbElement
        );

        if (filter is not null)
        {
            function = new PdbFilteredAggregateExpression((SqlFunctionExpression)function, filter);
        }

        return over ? new PdbOverExpression((SqlFunctionExpression)function) : function;
    }

    private PdbBoolExpression BuildQueryBuilderFunction(SqlExpression column, SqlExpression query)
    {
        column = _sqlExpressionFactory.ApplyDefaultTypeMapping(column);
        query = _sqlExpressionFactory.ApplyDefaultTypeMapping(query);
        return new PdbBoolExpression(column, query, PdbOperatorType.Function);
    }
}
