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
    private readonly RelationalTypeMapping _stringArrayTypeMapping;

    public Translator(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource
    )
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _jsonbTypeMapping = typeMappingSource.FindMapping(typeof(JsonElement), "jsonb")!;
        _stringArrayTypeMapping = typeMappingSource.FindMapping(typeof(string[]))!;
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
            nameof(Pdb.Exists) => _sqlExpressionFactory.Function(
                name: "pdb.exists",
                nullable: false,
                arguments: [],
                argumentsPropagateNullability: [],
                returnType: typeof(bool)
            ),
            nameof(Pdb.Parse) => _sqlExpressionFactory.Function(
                name: "pdb.parse",
                nullable: false,
                arguments: [arguments[0]],
                argumentsPropagateNullability: [false],
                returnType: typeof(bool)
            ),
            nameof(Pdb.Regex) => _sqlExpressionFactory.Function(
                name: "pdb.regex",
                nullable: false,
                arguments: [arguments[0]],
                argumentsPropagateNullability: [false],
                returnType: typeof(bool)
            ),
            nameof(Pdb.RegexPhrase) => BuildRegexPhrase(arguments),
            nameof(Pdb.RangeTerm) => BuildRangeTerm(arguments),
            nameof(Pdb.PhrasePrefix) => BuildPhrasePrefix(arguments),
            nameof(ParadeDbFunctionsExtensions.Snippet) => BuildSnippet(arguments),
            nameof(ParadeDbFunctionsExtensions.Snippets) => BuildSnippets(arguments),
            nameof(ParadeDbFunctionsExtensions.SnippetPositions) => _sqlExpressionFactory.Function(
                name: "pdb.snippet_positions",
                nullable: true,
                arguments: [arguments[1]],
                argumentsPropagateNullability: [false],
                returnType: typeof(int[,])
            ),
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

    private SqlExpression? BuildPhrasePrefix(IReadOnlyList<SqlExpression> arguments)
    {
        List<SqlExpression> args = [arguments[0]];

        // If the user didn't pass in max_expansions, let the DB do the defaulting
        if (arguments[1] is not SqlConstantExpression { Value: null })
        {
            args.Add(arguments[1]);
        }

        return _sqlExpressionFactory.Function(
            name: "pdb.phrase_prefix",
            nullable: false,
            arguments: args,
            argumentsPropagateNullability: new bool[args.Count],
            returnType: typeof(bool)
        );
    }

    private SqlExpression? BuildRegexPhrase(IReadOnlyList<SqlExpression> arguments)
    {
        List<SqlExpression> args = [_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0])];
        List<string?> argNames = [null];

        if (arguments[1] is not SqlConstantExpression { Value: null })
        {
            args.Add(_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[1]));
            argNames.Add("slop");
        }

        if (arguments[2] is not SqlConstantExpression { Value: null })
        {
            args.Add(_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]));
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

    private SqlExpression? BuildRangeTerm(IReadOnlyList<SqlExpression> arguments)
    {
        List<SqlExpression> args = [_sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[0])];

        if (arguments.Count > 1)
        {
            args.Add(
                arguments[1] is SqlConstantExpression { Value: RangeTermRelation relation }
                    ? _sqlExpressionFactory.Constant(relation.ToString())
                    : arguments[1]
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
        var options =
            arguments.Count == 3
                ? (SnippetOptions?)((SqlConstantExpression)arguments[2]).Value
                : null;
        List<SqlExpression> args = [arguments[1]];
        List<string?> argNames = [null];

        Add(options?.startTag, "start_tag");
        Add(options?.endTag, "end_tag");
        Add(options?.maxNumChars, "max_num_chars");

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
    }

    private SqlExpression? BuildSnippets(IReadOnlyList<SqlExpression> arguments)
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

        return PgFunctionExpression.CreateWithNamedArguments(
            name: "pdb.snippets",
            arguments: args,
            argumentNames: argNames,
            nullable: true,
            argumentsPropagateNullability: new bool[args.Count],
            builtIn: false,
            type: typeof(string[]),
            typeMapping: _stringArrayTypeMapping
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
            typeMapping: _jsonbTypeMapping
        );

        if (filter is not null)
        {
            function = new PdbFilteredAggregateExpression((SqlFunctionExpression)function, filter);
        }

        return over ? new PdbOverExpression((SqlFunctionExpression)function) : function;
    }

    private SqlExpression BuildQueryBuilderFunction(SqlExpression column, SqlExpression query)
    {
        column = _sqlExpressionFactory.ApplyDefaultTypeMapping(column);
        query = _sqlExpressionFactory.ApplyDefaultTypeMapping(query);
        return new PdbBoolExpression(column, query, PdbOperatorType.Function);
    }
}
