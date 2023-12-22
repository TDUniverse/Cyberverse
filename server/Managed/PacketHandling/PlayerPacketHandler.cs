using Cyberverse.Server.NativeLayer.Protocol.Serverbound;
using Cyberverse.Server.Services;
using Cyberverse.Server.Types;
using NLog;

namespace Cyberverse.Server.PacketHandling;

public class PlayerPacketHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly TypedPacketHandler<PlayerJoinWorld> _playerJoinHandler;
    private readonly TypedPacketHandler<PlayerPositionUpdate> _playerMoveHandler;
    private readonly TypedPacketHandler<PlayerSpawnCar> _playerSpawnCarHandler;
    private EntityTracker? _tracker = null;
    private PlayerService? _players = null;

    public PlayerPacketHandler()
    {
        _playerJoinHandler = new TypedPacketHandler<PlayerJoinWorld>(HandleJoinWorld);
        _playerMoveHandler = new TypedPacketHandler<PlayerPositionUpdate>(HandlePositionUpdate);
        _playerSpawnCarHandler = new TypedPacketHandler<PlayerSpawnCar>(HandleSpawnCar);
    }

    protected void HandleJoinWorld(GameServer server, EMessageTypeServerbound messageType, byte channelId, uint connectionId, PlayerJoinWorld content)
    {
        if (_players!.ConnectedPlayers.TryGetValue(connectionId, out var player))
        {
            Logger.Trace("Player {0} joined the world at ({1}, {2}, {3})", player.Name, 
                content.position.x, content.position.y, content.position.z);

            var recordId = new Random().NextSingle() > 0.5f ? "Character.Judy" : "Character.Panam";
            var entity = server.EntityService.CreateEntity(recordId);
            entity.WorldTransform = content.position; // Spawn the entity at the right spot already
            entity.NetworkIdOwner = player.ConnectionId;
                
            // Since we only call the tracking for moved entities, force sync every entity to that player:
            _tracker!.InitialSpawnForPlayer(player);
        }
        else
        {
            Logger.Warn($"Could not find player with id {connectionId} in the player list.");
        }
    }

    protected void HandlePositionUpdate(GameServer server, EMessageTypeServerbound messageType, byte channelId,
        uint connectionId, PlayerPositionUpdate content)
    {
        // // TODO: Implement differently, don't teleport.
        // TeleportEntity teleportEntity = default;
        // teleportEntity.networkedEntityId = 0;
        // teleportEntity.targetPosition = positionUpdate.worldTransform;
        // teleportEntity.yaw = positionUpdate.yaw;
        //                         
        // Message outputMessage = default;
        // outputMessage.MessageTypeClientBound(EMessageTypeClientbound.TeleportEntity);
        // outputMessage.channelId = 1;
        // outputMessage.connectionId = message.connectionId;
        // outputMessage.MarshalFrom(teleportEntity);
        //
        // EnqueueMessage(outputMessage);

        var playerEntities =
            server.EntityService.SpawnedEntities.Values.Where(tty => tty.NetworkIdOwner == connectionId);
        foreach (var entity in playerEntities)
        {
            //Console.WriteLine($"Move {player.Name} ({player.ConnectionId} / {message.connectionId}) to ({positionUpdate.worldTransform.x}, {positionUpdate.worldTransform.y}, {positionUpdate.worldTransform.z})");
            
            // TODO: sometimes the game sends faulty updates to ~0, 3.16, 0.
            // TODO: This is happening before the savegame has loaded. A better solution is to prevent updating networkEntityIds that are not spawned yet,
            // because what happens here is that player.NetworkedEntityId is 0 because it has no entity yet, thus moving the old player.
            // Note: this should already be solved.
            if (content.worldTransform.z == 0 && content.worldTransform.x == 0.010673523f)
            {
                // TODO: we should really remove that.
                Logger.Trace("Skipping position update");
                return;
            }
                    
            entity.WorldTransform = content.worldTransform;
            entity.Yaw = content.yaw;
            _tracker!.UpdateTrackingOf(entity);
        }
    }

    protected void HandleSpawnCar(GameServer server, EMessageTypeServerbound messageType, byte channelId,
        uint connectionId, PlayerSpawnCar content)
    {
        var hash = RecordIdUtils.RecordIdToCrcHash(content.recordId);
        var len = RecordIdUtils.RecordIdToNameLength(content.recordId);
        
        Logger.Trace($"Player spawned a {hash} [{len}] at {content.worldTransform}");
        
        // Try to find existing cars for that owner.
        var existingVehicles = server.EntityService.SpawnedEntities
            .Select(x => x.Value)
            .Where(x => x.NetworkIdOwner == connectionId)
            .Where(x => x.IsVehicle)
            .ToList();

        foreach (var vehicle in existingVehicles)
        {
            vehicle.NetworkIdOwner = 0;
            _tracker!.StopTrackingOf(vehicle);
            server.EntityService.RemoveEntity(vehicle.NetworkedEntityId);
        }

        var entity = server.EntityService.CreateEntity(content.recordId);
        entity.WorldTransform = content.worldTransform; // Spawn the entity at the right spot already
        entity.Yaw = content.yaw;
        entity.NetworkIdOwner = connectionId;
        entity.IsVehicle = true;

        _tracker!.UpdateTrackingOf(entity);
        
    }

    public void RegisterOnServer(GameServer server)
    {
        _tracker = server.EntityTracker; // TODO: Service registry or even using DI
        _players = server.PlayerService;
        
        server.AddPacketHandler(EMessageTypeServerbound.PlayerJoinWorld, _playerJoinHandler.HandlePacket);
        server.AddPacketHandler(EMessageTypeServerbound.PlayerPositionUpdate, _playerMoveHandler.HandlePacket);
        server.AddPacketHandler(EMessageTypeServerbound.PlayerSpawnCar, _playerSpawnCarHandler.HandlePacket);
    }
}