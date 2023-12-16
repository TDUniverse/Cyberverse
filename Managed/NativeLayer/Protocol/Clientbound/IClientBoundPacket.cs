using CyberM.Server.NativeLayer.Protocol.Serverbound;

namespace CyberM.Server.NativeLayer.Protocol.Clientbound;

public interface IClientBoundPacket
{
    public EMessageTypeClientbound GetMessageType();
}