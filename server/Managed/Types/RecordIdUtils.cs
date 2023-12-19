namespace CyberM.Server.Types;

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
}