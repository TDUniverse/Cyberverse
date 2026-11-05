using System.Runtime.InteropServices;

namespace Cyberverse.Server.NativeLayer.Protocol.Clientbound;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct EquipItemEntity: IClientBoundPacket
{
    public ulong NetworkedEntityId;
    public ulong Slot;
    public ulong ItemId;
    // C++ bools are 1 byte, but marshaling defaults to the 4 byte Win32 BOOL.
    [MarshalAs(UnmanagedType.U1)] public bool isWeapon;
    [MarshalAs(UnmanagedType.U1)] public bool isUnequipping;

    public EMessageTypeClientbound GetMessageType()
    {
        return EMessageTypeClientbound.EquipItemEntity;
    }
}