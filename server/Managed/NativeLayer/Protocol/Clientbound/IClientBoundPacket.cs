using Cyberverse.Server.NativeLayer.Protocol.Serverbound;

namespace Cyberverse.Server.NativeLayer.Protocol.Clientbound;

public interface IClientBoundPacket
{
    public EMessageTypeClientbound GetMessageType();
}