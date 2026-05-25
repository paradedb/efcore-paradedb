using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ParadeDB.EntityFrameworkCore.Extensions;

[ExcludeFromCodeCoverage]
public static class PdbMoreLikeThisQueryExtensions
{
    extension(PdbMoreLikeThisQuery source)
    {
        public PdbMoreLikeThisQuery Fields(params string[] fields) =>
            throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Fields)));

        public PdbMoreLikeThisQuery MinTermFrequency(int value) =>
            throw new InvalidOperationException(
                CoreStrings.FunctionOnClient(nameof(MinTermFrequency))
            );

        public PdbMoreLikeThisQuery MinDocFrequency(int value) =>
            throw new InvalidOperationException(
                CoreStrings.FunctionOnClient(nameof(MinDocFrequency))
            );

        public PdbMoreLikeThisQuery MaxDocFrequency(int value) =>
            throw new InvalidOperationException(
                CoreStrings.FunctionOnClient(nameof(MaxDocFrequency))
            );

        public PdbMoreLikeThisQuery MaxQueryTerms(int value) =>
            throw new InvalidOperationException(
                CoreStrings.FunctionOnClient(nameof(MaxQueryTerms))
            );

        public PdbMoreLikeThisQuery MinWordLength(int value) =>
            throw new InvalidOperationException(
                CoreStrings.FunctionOnClient(nameof(MinWordLength))
            );

        public PdbMoreLikeThisQuery MaxWordLength(int value) =>
            throw new InvalidOperationException(
                CoreStrings.FunctionOnClient(nameof(MaxWordLength))
            );

        public PdbMoreLikeThisQuery Stopwords(params string[] stopwords) =>
            throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Stopwords)));
    }
}
