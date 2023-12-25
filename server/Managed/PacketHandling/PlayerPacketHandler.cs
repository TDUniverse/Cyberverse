using Cyberverse.Server.NativeLayer.Protocol.Serverbound;
using Cyberverse.Server.Services;
using Cyberverse.Server.Types;
using NLog;

namespace Cyberverse.Server.PacketHandling;

public class PlayerPacketHandler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly Random Random = new();
    private static readonly string[] PossiblePlayerNpcChoice = ["Character.Judy", "Character.Panam", "Character.Alt", 
        "Character.Clair", "Character.Dexter", "Character.Evelyn", "Character.Hanako", "Character.Jackie", 
        "Character.Mama_Welles", "Character.Misty", "Character.Stout", "Character.Takemura", "Character.Tbug",
        "Character.wakako_okada", "Character.Yorinobu"];
    
    private readonly TypedPacketHandler<PlayerJoinWorld> _playerJoinHandler;
    private readonly TypedPacketHandler<PlayerPositionUpdate> _playerMoveHandler;
    private readonly TypedPacketHandler<PlayerSpawnCar> _playerSpawnCarHandler;
    private readonly TypedPacketHandler<PlayerUnmountCar> _playerUnmountHandler;
    private readonly TypedPacketHandler<PlayerEquipItem> _playerEquipHandler;
    private readonly TypedPacketHandler<PlayerShoot> _playerShootHandler;
    private EntityTracker? _tracker = null;
    private PlayerService? _players = null;

    public PlayerPacketHandler()
    {
        _playerJoinHandler = new TypedPacketHandler<PlayerJoinWorld>(HandleJoinWorld);
        _playerMoveHandler = new TypedPacketHandler<PlayerPositionUpdate>(HandlePositionUpdate);
        _playerSpawnCarHandler = new TypedPacketHandler<PlayerSpawnCar>(HandleSpawnCar);
        _playerUnmountHandler = new TypedPacketHandler<PlayerUnmountCar>(HandleUnmountCar);
        _playerEquipHandler = new TypedPacketHandler<PlayerEquipItem>(HandleEquip);
        _playerShootHandler = new TypedPacketHandler<PlayerShoot>(HandleShoot);
    }

    protected void HandleJoinWorld(GameServer server, EMessageTypeServerbound messageType, byte channelId, uint connectionId, PlayerJoinWorld content)
    {
        if (_players!.ConnectedPlayers.TryGetValue(connectionId, out var player))
        {
            Logger.Trace("Player {0} joined the world at ({1}, {2}, {3})", player.Name, 
                content.position.x, content.position.y, content.position.z);

            var choice = Random.Next(PossiblePlayerNpcChoice.Length);
            var entity = server.EntityService.CreateEntity(PossiblePlayerNpcChoice[choice]);
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
        
        DespawnAllVehiclesForPlayer(server, connectionId);

        var entity = server.EntityService.CreateEntity(content.recordId);
        entity.WorldTransform = content.worldTransform; // Spawn the entity at the right spot already
        entity.Yaw = content.yaw;
        entity.NetworkIdOwner = connectionId;
        entity.IsVehicle = true;

        _tracker!.UpdateTrackingOf(entity);
    }

    private void DespawnAllVehiclesForPlayer(GameServer server, uint connectionId)
    {
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
    }

    protected void HandleUnmountCar(GameServer server, EMessageTypeServerbound messageType, byte channelId,
        uint connectionId, PlayerUnmountCar content)
    {
        DespawnAllVehiclesForPlayer(server, connectionId);
    }

    private void HandleEquip(GameServer server, EMessageTypeServerbound messageType, byte channelId,
        uint connectionId, PlayerEquipItem content)
    {
        Logger.Info($"Player did (un: {content.isUnequipping})equip an item (slot {content.slot}). Is it a weapon? {content.isWeapon}");
        if (content.isUnequipping)
        {
            // Try to find existing cars for that owner.
            var existingItems = server.EntityService.SpawnedEntities
                .Select(x => x.Value)
                .Where(x => x.NetworkIdOwner == connectionId)
                .Where(x => x.RecordId == content.itemId)
                .ToList();

            // TODO: As long as we don't have NetworkedEntityIds, we need to unequip all of those.
            foreach (var item in existingItems)
            {
                item.NetworkIdOwner = 0;
                _tracker!.StopTrackingOf(item);
                server.EntityService.RemoveEntity(item.NetworkedEntityId);
            }
        }
        else
        {
            // TODO: How are we supposed to flag things as items? dedicated maps? But it'll still be entities in the end.
            // Also this is relevant when we have our own EquipItemEntity. Right now they are entities in the world.
            var playerEntity = server.EntityService.SpawnedEntities
                .Select(x => x.Value)
                .FirstOrDefault(x => x.NetworkIdOwner == connectionId);

            if (playerEntity == null)
            {
                return;
            }
            
            var entity = server.EntityService.CreateEntity(content.itemId);
            entity.WorldTransform = playerEntity.WorldTransform; // TODO
            entity.Yaw = playerEntity.Yaw; // TODO
            entity.NetworkIdOwner = connectionId;
            entity.IsVehicle = false;

            _tracker!.UpdateTrackingOf(entity);
        }
    }

    private void HandleShoot(GameServer server, EMessageTypeServerbound messageType, byte channelId,
        uint connectionId, PlayerShoot content)
    {
        Logger.Warn($"Shots fired! {content.itemIdWeapon} at {content.startPoint}");
    }

    public void RegisterOnServer(GameServer server)
    {
        _tracker = server.EntityTracker; // TODO: Service registry or even using DI
        _players = server.PlayerService;
        
        server.AddPacketHandler(EMessageTypeServerbound.PlayerJoinWorld, _playerJoinHandler.HandlePacket);
        server.AddPacketHandler(EMessageTypeServerbound.PlayerPositionUpdate, _playerMoveHandler.HandlePacket);
        server.AddPacketHandler(EMessageTypeServerbound.PlayerSpawnCar, _playerSpawnCarHandler.HandlePacket);
        server.AddPacketHandler(EMessageTypeServerbound.PlayerUnmountCar, _playerUnmountHandler.HandlePacket);
        server.AddPacketHandler(EMessageTypeServerbound.PlayerEquipItem, _playerEquipHandler.HandlePacket);
        server.AddPacketHandler(EMessageTypeServerbound.PlayerShoot, _playerShootHandler.HandlePacket);
    }
}
