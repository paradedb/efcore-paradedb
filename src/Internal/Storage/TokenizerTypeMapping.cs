using Microsoft.EntityFrameworkCore.Storage;

namespace ParadeDB.EntityFrameworkCore.Internal.Storage;

internal sealed class TokenizerTypeMapping : RelationalTypeMapping
{
    public TokenizerTypeMapping(Tokenizer tokenizer, bool asArray = false)
        : base(Create(tokenizer, asArray)) { }

    private static RelationalTypeMappingParameters Create(Tokenizer tokenizer, bool asArray)
    {
        if (asArray)
        {
            return new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(string[])),
                $"{tokenizer}::text[]"
            );
        }

        return new RelationalTypeMappingParameters(
            new CoreTypeMappingParameters(typeof(string)),
            $"{tokenizer}"
        );
    }

    private TokenizerTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters) { }

    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters) =>
        new TokenizerTypeMapping(parameters);
}
