using System.Runtime.InteropServices;
using System.Text;

namespace Cyberverse.Server.NativeLayer.Protocol.Serverbound;

/// <summary>
/// Mirrors the fixed size copy the native layer makes of InitAuthServerBound, whose std::string C# can't read.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct InitAuthServerBoundCSharp
{
    // UTF-8 and only null terminated when shorter than the buffer. CharSet.Ansi would decode it with the system code
    // page on Windows, so it's read as bytes instead.
#pragma warning disable CS0649 // Only ever filled by the marshaler
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
    private byte[]? username;
#pragma warning restore CS0649
    // TODO: proof
    public uint protocol_version;

    public string Username
    {
        get
        {
            var bytes = username ?? [];
            var length = Array.IndexOf(bytes, (byte)0);
            return Encoding.UTF8.GetString(bytes, 0, length < 0 ? bytes.Length : length);
        }
    }
}
