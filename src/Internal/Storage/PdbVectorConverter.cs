using Npgsql.Internal;

namespace ParadeDB.EntityFrameworkCore.Internal.Storage;

internal sealed class PdbVectorConverter : PgStreamingConverter<float[]>
{
    public override float[] Read(PgReader reader)
    {
        if (reader.ShouldBuffer(2 * sizeof(ushort)))
        {
            reader.Buffer(2 * sizeof(ushort));
        }

        var dimensions = reader.ReadUInt16();
        reader.ReadUInt16();

        var vector = new float[dimensions];
        for (var i = 0; i < dimensions; i++)
        {
            if (reader.ShouldBuffer(sizeof(float)))
            {
                reader.Buffer(sizeof(float));
            }

            vector[i] = reader.ReadFloat();
        }

        return vector;
    }

    public override async ValueTask<float[]> ReadAsync(
        PgReader reader,
        CancellationToken cancellationToken = default
    )
    {
        if (reader.ShouldBuffer(2 * sizeof(ushort)))
        {
            await reader.BufferAsync(2 * sizeof(ushort), cancellationToken).ConfigureAwait(false);
        }

        var dimensions = reader.ReadUInt16();
        reader.ReadUInt16();

        var vector = new float[dimensions];
        for (var i = 0; i < dimensions; i++)
        {
            if (reader.ShouldBuffer(sizeof(float)))
            {
                await reader.BufferAsync(sizeof(float), cancellationToken).ConfigureAwait(false);
            }

            vector[i] = reader.ReadFloat();
        }

        return vector;
    }

    public override Size GetSize(SizeContext context, float[] value, ref object? writeState) =>
        2 * sizeof(ushort) + sizeof(float) * value.Length;

    public override void Write(PgWriter writer, float[] value)
    {
        if (writer.ShouldFlush(2 * sizeof(ushort)))
        {
            writer.Flush();
        }

        writer.WriteUInt16(Convert.ToUInt16(value.Length));
        writer.WriteUInt16(0);

        foreach (var element in value)
        {
            if (writer.ShouldFlush(sizeof(float)))
            {
                writer.Flush();
            }

            writer.WriteFloat(element);
        }
    }

    public override async ValueTask WriteAsync(
        PgWriter writer,
        float[] value,
        CancellationToken cancellationToken = default
    )
    {
        if (writer.ShouldFlush(2 * sizeof(ushort)))
        {
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        writer.WriteUInt16(Convert.ToUInt16(value.Length));
        writer.WriteUInt16(0);

        foreach (var element in value)
        {
            if (writer.ShouldFlush(sizeof(float)))
            {
                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            writer.WriteFloat(element);
        }
    }
}
