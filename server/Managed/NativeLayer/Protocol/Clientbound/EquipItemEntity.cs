using System.Runtime.InteropServices;

namespace Cyberverse.Server.NativeLayer.Protocol.Clientbound;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct EquipItemEntity: IClientBoundPacket
{
    public ulong NetworkedEntityId;
    public ulong Slot;
    public ulong ItemId;
    public bool isWeapon;
    public bool isUnequipping;

    public EMessageTypeClientbound GetMessageType()
    {
        return EMessageTypeClientbound.EquipItemEntity;
    }
}