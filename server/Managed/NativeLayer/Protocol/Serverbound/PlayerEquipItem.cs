using System.Runtime.InteropServices;

namespace Cyberverse.Server.NativeLayer.Protocol.Serverbound;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct PlayerEquipItem
{
    public ulong slot;
    public ulong itemId;
    // C++ bools are 1 byte, but marshaling defaults to the 4 byte Win32 BOOL.
    [MarshalAs(UnmanagedType.U1)] public bool isWeapon;
    [MarshalAs(UnmanagedType.U1)] public bool isUnequipping;
}