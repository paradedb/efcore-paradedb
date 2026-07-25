namespace ParadeDB.EntityFrameworkCore;

public enum VectorMetric
{
    L2,
    Cosine,
    InnerProduct,
}

internal static class VectorMetricExtensions
{
    public static string ToOpclass(this VectorMetric metric) =>
        metric switch
        {
            VectorMetric.L2 => "vector_l2_ops",
            VectorMetric.Cosine => "vector_cosine_ops",
            VectorMetric.InnerProduct => "vector_ip_ops",
            _ => throw new ArgumentOutOfRangeException(nameof(metric)),
        };
}
