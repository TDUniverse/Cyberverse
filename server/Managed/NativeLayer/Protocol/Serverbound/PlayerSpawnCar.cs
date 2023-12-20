using System.Runtime.InteropServices;
using Cyberverse.Server.NativeLayer.Protocol.Common;

namespace Cyberverse.Server.NativeLayer.Protocol.Serverbound;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct PlayerSpawnCar
{
    public Vector3 worldTransform;
    public float yaw;
    public ulong recordId; // u32 crc32, u8 strlen (for easier lookup, I guess), and 3 bytes dbOffset (don't care)
}