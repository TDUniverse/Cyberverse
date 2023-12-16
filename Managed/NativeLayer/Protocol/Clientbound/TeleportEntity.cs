using System.Runtime.InteropServices;
using CyberM.Server.NativeLayer.Protocol.Common;
using CyberM.Server.NativeLayer.Protocol.Serverbound;

namespace CyberM.Server.NativeLayer.Protocol.Clientbound;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
// TODO: There will be better packets, containing target locations for interpolation, with timestamps.
public struct TeleportEntity: IClientBoundPacket
{
    public ulong networkedEntityId;
    public Vector3 targetPosition;
    public float yaw; // NPCs (i.e. players) only have a yaw that you can set anyway, not a full rotation.
    
    public EMessageTypeClientbound GetMessageType() => EMessageTypeClientbound.TeleportEntity;
}