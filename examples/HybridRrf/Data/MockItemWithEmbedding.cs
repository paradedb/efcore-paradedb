using Pgvector;
using Shared;

namespace HybridRrf.Data;

public class MockItemWithEmbedding : MockItem
{
    public Vector? Embedding { get; set; }
}
