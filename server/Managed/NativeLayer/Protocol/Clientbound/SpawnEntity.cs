using System.Runtime.InteropServices;
using Cyberverse.Server.NativeLayer.Protocol.Common;
using Cyberverse.Server.NativeLayer.Protocol.Serverbound;

namespace Cyberverse.Server.NativeLayer.Protocol.Clientbound;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct SpawnEntity: IClientBoundPacket
{
    public ulong networkedEntityId;
    public ulong recordId;
    public Vector3 spawnPosition;

    public EMessageTypeClientbound GetMessageType() => EMessageTypeClientbound.SpawnEntity;
}