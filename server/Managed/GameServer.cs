﻿using Cyberverse.Server.NativeLayer;
using Cyberverse.Server.NativeLayer.Protocol.Clientbound;
using Cyberverse.Server.NativeLayer.Protocol.Serverbound;
using Cyberverse.Server.PacketHandling;
using Cyberverse.Server.Services;
using NLog;

namespace Cyberverse.Server;

public class GameServer: NativeGameServer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly Dictionary<EMessageTypeServerbound, HandlePacket> _packetHandlers = new();

    public readonly EntityService EntityService;
    public readonly EntityTracker EntityTracker;
    public readonly PlayerService PlayerService = new();

    public GameServer(ushort listeningPort) : base(listeningPort)
    {
        EntityTracker = new EntityTracker(this);
        EntityService = new EntityService();
    }

    public void AddPacketHandler(EMessageTypeServerbound messageType, HandlePacket packetHandler)
    {
        _packetHandlers.Add(messageType, packetHandler);
    }

    protected override void OnConnectionStateChange(ESteamNetworkingConnectionState state, uint connectionId)
    {
        Logger.Trace($"{connectionId} changed state to {state}");

        // TODO: More handling of player connection states. Cleanup routines, pre-auth states, ...
        if (state is ESteamNetworkingConnectionState.ClosedByPeer or ESteamNetworkingConnectionState.ProblemDetectedLocally)
        {
            if (!PlayerService.ConnectedPlayers.TryGetValue(connectionId, out var player))
            {
                // someone connected but never signed in
                return;
            }
            
            Logger.Info($"Player {player.Name} disconnected");
            var playerEntities = EntityService.SpawnedEntities
                .Select(x => x.Value)
                .Where(x => x.NetworkIdOwner == connectionId)
                .ToList();
                
            foreach (var playerEntity in playerEntities)
            {
                EntityTracker.StopTrackingOf(playerEntity);
                EntityService.RemoveEntity(playerEntity.NetworkedEntityId);
                EntityService.SpawnedEntities.Remove(playerEntity.NetworkedEntityId);
            }

            PlayerService.ConnectedPlayers.Remove(connectionId);
        }
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        var msg = PollIncomingMessages();
        while (msg.HasData())
        {
            ProcessMessage(msg);
            msg = PollIncomingMessages();
        }
    }

    public void EnqueueMessage<T>(EMessageTypeClientbound messageType, uint connectionId, byte channelId, T content) where T: struct
    {
        Message outputMessage = default;
        outputMessage.MessageTypeClientBound(messageType);
        outputMessage.channelId = channelId;
        outputMessage.connectionId = connectionId;
        
        outputMessage.MarshalFrom(content);
        EnqueueMessage(outputMessage);
    }
    
    // TODO: Move somewhere.
    public const uint PROTOCOL_VERSION_CURRENT = 0x0;
    
    protected virtual void ProcessMessage(Message message)
    {
        var messageType = message.MessageTypeServerBound();
        if (_packetHandlers.TryGetValue(messageType, out var handler))
        {
            // Hint: If we wanted to invoke multiple handlers, we'd really need a different approach, since
            // TypedPacketHandler will try to marshal the message multiple times, which wouldn't work, so we kinda need
            // to expose the API that TypedPacketHandler has, kinda as interface or something, but then we still have 
            // the problem of structs not being objects, so type safety is questionable.
            handler.Invoke(this, messageType, ref message);
            return;
        }
        
        Logger.Warn($"No message handler for type {messageType}");
        if (message.messageType == 2)
        {
            var actionTracked = message.MarshalTo<PlayerActionTracked>();
            // if (actionTracked.action == EPlayerAction.ActionJump)
            // {
            //     SpawnEntity spawnEntity = default;
            //     spawnEntity.networkedEntityId = 0;
            //     spawnEntity.recordId = new Random().NextSingle() > 0.5f ? "Character.Judy" : "Character.Panam";
            //     spawnEntity.spawnPosition = actionTracked.worldTransform;
            //     
            //     Message outputMessage = default;
            //     outputMessage.MessageTypeClientBound(EMessageTypeClientbound.SpawnEntity);
            //     outputMessage.channelId = 1;
            //     outputMessage.connectionId = message.connectionId;
            //
            //     outputMessage.MarshalFrom(spawnEntity);
            //     EnqueueMessage(outputMessage);
            // }
            // else if (actionTracked.action == EPlayerAction.ActionRangedAttack)
            // {
            // }
        }
    }
}