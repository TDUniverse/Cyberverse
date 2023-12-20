using Cyberverse.Server.NativeLayer;
using Cyberverse.Server.NativeLayer.Protocol.Serverbound;

namespace Cyberverse.Server.PacketHandling;

public delegate void HandlePacket(GameServer server, EMessageTypeServerbound messageType, ref Message message);

public delegate void HandlePacketTyped<T>(GameServer server, EMessageTypeServerbound messageType, byte channelId,
    uint connectionId, T content) where T : struct;

public class TypedPacketHandler<T>(HandlePacketTyped<T> handler) where T : struct
{
    public void HandlePacket(GameServer server, EMessageTypeServerbound messageType, ref Message message)
    {
        handler(server, messageType, message.channelId, message.connectionId, message.MarshalTo<T>());
    }
}
