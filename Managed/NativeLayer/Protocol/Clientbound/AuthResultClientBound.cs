using System.Runtime.InteropServices;
using CyberM.Server.NativeLayer.Protocol.Serverbound;

namespace CyberM.Server.NativeLayer.Protocol.Clientbound;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct AuthResultClientBound: IClientBoundPacket
{
    public EAuthResult auth_result;
    public uint protocol_version;

    public EMessageTypeClientbound GetMessageType() => EMessageTypeClientbound.InitAuthResult;
}