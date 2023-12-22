using System.Runtime.InteropServices;
using Cyberverse.Server.NativeLayer.Protocol.Common;
using Cyberverse.Server.NativeLayer.Protocol.Serverbound;

namespace Cyberverse.Server.NativeLayer.Protocol.Clientbound;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct DestroyEntity: IClientBoundPacket
{
    public ulong networkedEntityId;
    public EMessageTypeClientbound GetMessageType() => EMessageTypeClientbound.DestroyEntity;
}