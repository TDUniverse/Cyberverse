using System.Runtime.InteropServices;

namespace Cyberverse.Server.NativeLayer.Protocol.Serverbound;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct PlayerEquipItem
{
    public ulong slot;
    public ulong itemId;
    public bool isWeapon;
    public bool isUnequipping; // TODO: For some reason this flag is always false. Is this a packing issue because the bools got bitpacked?
}