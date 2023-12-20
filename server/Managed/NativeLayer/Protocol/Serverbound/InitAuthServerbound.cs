using System.Runtime.InteropServices;

namespace Cyberverse.Server.NativeLayer.Protocol.Serverbound;

[StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Ansi)]
public struct InitAuthServerbound
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
    public string username; // TODO: limit to 0x255
    // TODO: proof
    public uint protocol_version;
}