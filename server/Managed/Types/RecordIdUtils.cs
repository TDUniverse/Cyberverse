using System.IO.Hashing;
using System.Text;

namespace Cyberverse.Server.Types;

public static class RecordIdUtils
{
    public static uint RecordIdToCrcHash(ulong recordId)
    {
        return (uint)(recordId & 0xFFFFFFFFu);
    }

    public static byte RecordIdToNameLength(ulong recordId)
    {
        return (byte)(recordId >> 32 & 0xFF);
    }

    public static ulong ToRecordId(string recordId)
    {
        // We're eating the instantiation offset here, but at least we're multithreading safe then
        var crc32 = new Crc32();
        crc32.Append(Encoding.ASCII.GetBytes(recordId));
        var hash = crc32.GetCurrentHashAsUInt32();
        var strlen = (ulong)recordId.Length;

        if (strlen > 0xFF)
        {
            throw new ArgumentException("String record too long. Limit is 255 characters", nameof(recordId));
        }

        return (strlen << 32) | hash;
    }
}