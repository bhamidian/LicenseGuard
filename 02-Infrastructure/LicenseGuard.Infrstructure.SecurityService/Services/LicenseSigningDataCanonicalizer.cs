using System.Buffers.Binary;
using System.Text;
using LicenseGuard.Domain.Records;

namespace LicenseGuard.Infrstructure.SecurityService;

public static class LicenseSigningDataCanonicalizer
{
    private static readonly byte[] DomainSeparator = Encoding.ASCII.GetBytes("LicenseGuard\0SigningData\0v2\0");

    public static byte[] Canonicalize(LicenseSigningData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        using var stream = new MemoryStream(512);
        stream.Write(DomainSeparator);

        var key = Encoding.UTF8.GetBytes(data.LicenseKey);
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, key.Length);
        stream.Write(length);
        stream.Write(key);

        WriteGuid(stream, data.CustomerId);
        WriteGuid(stream, data.ProductId);
        WriteGuid(stream, data.PlanId);

        WriteString(stream, data.PolicyVersion);

        WriteInt32(stream, data.Features.Count);
        foreach (var feature in data.Features.OrderBy(x => x.FeatureId))
        {
            WriteGuid(stream, feature.FeatureId);
            WriteString(stream, feature.Code);
            stream.WriteByte(feature.IsEnabled ? (byte)1 : (byte)0);
        }

        WriteInt32(stream, data.Limits.Count);
        foreach (var limit in data.Limits.OrderBy(x => x.Code, StringComparer.Ordinal))
        {
            WriteString(stream, limit.Code);
            WriteString(stream, limit.Value.ToString("G29", System.Globalization.CultureInfo.InvariantCulture));
            WriteString(stream, limit.Unit ?? string.Empty);
        }

        Span<byte> ticks = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(ticks, ToUtcTicks(data.StartDate));
        stream.Write(ticks);
        BinaryPrimitives.WriteInt64BigEndian(ticks, ToUtcTicks(data.ExpirationDate));
        stream.Write(ticks);
        return stream.ToArray();
    }

    private static void WriteGuid(Stream stream, Guid value)
    {
        Span<byte> bytes = stackalloc byte[16];
        value.TryWriteBytes(bytes, bigEndian: true, out _);
        stream.Write(bytes);
    }

    private static void WriteString(Stream stream, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteInt32(stream, bytes.Length);
        stream.Write(bytes);
    }

    private static void WriteInt32(Stream stream, int value)
    {
        Span<byte> bytes = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        stream.Write(bytes);
    }

    private static long ToUtcTicks(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value.Ticks,
        DateTimeKind.Local => value.ToUniversalTime().Ticks,
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc).Ticks
    };
}
