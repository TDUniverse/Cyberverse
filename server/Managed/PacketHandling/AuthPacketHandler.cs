using CyberM.Server.NativeLayer.Protocol.Clientbound;
using CyberM.Server.NativeLayer.Protocol.Serverbound;
using CyberM.Server.Services;
using CyberM.Server.Types;

namespace CyberM.Server.PacketHandling;

public class AuthPacketHandler
{
    private readonly TypedPacketHandler<InitAuthServerbound> _initAuthHandler;
    private PlayerService? _players = null;

    public AuthPacketHandler()
    {
        _initAuthHandler = new TypedPacketHandler<InitAuthServerbound>(HandleInitAuth);
    }

    protected virtual void HandleInitAuth(GameServer server, EMessageTypeServerbound messageType, byte channelId, uint connectionId, InitAuthServerbound content)
    {
        Console.WriteLine($"Connection request from {content.username}");
            
        AuthResultClientBound resultPacket = default;
        resultPacket.protocol_version = GameServer.PROTOCOL_VERSION_CURRENT;
            
        if (content.protocol_version != GameServer.PROTOCOL_VERSION_CURRENT)
        {
            resultPacket.auth_result = EAuthResult.VersionMismatch;
        }
        else
        {
            //const auto validation = m_authController.ValidateUser(init_auth->username);
            var validation = true;
            resultPacket.auth_result = validation ? EAuthResult.Ok : EAuthResult.ValidationFailed;
            // TODO: Check if there is an active player with that uuid already (that requires having non-username logins first, and also breaks my local dev for now!)
            _players!.ConnectedPlayers.Add(connectionId, new Player { Name = content.username, ConnectionId = connectionId });
        }

        Message outputMessage = default;
        outputMessage.channelId = 0;
        outputMessage.MessageTypeClientBound(EMessageTypeClientbound.InitAuthResult);
        outputMessage.connectionId = connectionId;
            
        outputMessage.MarshalFrom(resultPacket);
        server.EnqueueMessage(outputMessage);
    }

    public virtual void RegisterOnServer(GameServer server)
    {
        _players = server.PlayerService;
        server.AddPacketHandler(EMessageTypeServerbound.InitAuth, _initAuthHandler.HandlePacket);
    }
}