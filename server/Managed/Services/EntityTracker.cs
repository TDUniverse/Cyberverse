using CyberM.Server.NativeLayer.Protocol.Clientbound;
using CyberM.Server.NativeLayer.Protocol.Serverbound;
using CyberM.Server.Types;

namespace CyberM.Server.Services;

// TODO: This is just a hacked prototype, this can take any form of complexity and subtle bugs with it (and include dynamic visiblity)
public class EntityTracker
{
    private readonly GameServer _server;
    private readonly Dictionary<uint, List<ulong>> _trackedEntities = new();

    public EntityTracker(GameServer server)
    {
        _server = server;
    }
    
    private void SendSpawnPacket(Entity entity, uint connectionId)
    {
        var spawnEntity = new SpawnEntity
        {
            networkedEntityId = entity.NetworkedEntityId,
            recordId = entity.RecordId,
            spawnPosition = entity.WorldTransform
        };
        _server.EnqueueMessage(EMessageTypeClientbound.SpawnEntity, connectionId, 1, spawnEntity);
    }

    public void OnStartTrackingEntity(Player player, Entity entity)
    {
        if (!_trackedEntities.TryGetValue(player.ConnectionId, out var list))
        {
            _trackedEntities.Add(player.ConnectionId, [ entity.NetworkedEntityId ]);
            SendSpawnPacket(entity, player.ConnectionId);
            return;
        }
        
        if (list.Contains(entity.NetworkedEntityId))
        {
            // Already tracked!
            return;
        }
            
        list.Add(entity.NetworkedEntityId);
        SendSpawnPacket(entity, player.ConnectionId);
    }

    public void UpdateTrackingFor(Entity entity)
    {
        foreach (var (connectionId, player) in _server.PlayerService.ConnectedPlayers)
        {
            // TODO: there may be more than one networkedEntity related to the player, so entities get a parentOwner reference.
            // Don't track yourself.
            if (player.NetworkedEntityId != entity.NetworkedEntityId)
            {
                UpdateTrackingFor(entity, player);
            }
        }
    }

    public void UpdateTrackingFor(Entity entity, Player player)
    {
        if (!player.NetworkedEntityId.HasValue ||
            !_server.EntityService.SpawnedEntities.TryGetValue(player.NetworkedEntityId.Value, out var playerEntity))
        {
            // Player not spawned at the moment
            return;
        }
        
        // TODO: Check for out of bounds.
        var playerPosition = playerEntity.WorldTransform;
        if (playerPosition.DistanceSquared(entity.WorldTransform) <= 100 * 100)
        {
            // OnStartTrackingEntity just skips on already tracked entities, so we can avoid checking that condition
            // twice and just call it directly.
            OnStartTrackingEntity(player, entity);

            // TODO: In the future, we may not want to immediately update entities that have been recently spawned,
            //  because if spawning and updating are different channels, update may arrive before spawning.
            //  Though, the client should ignore updates for non existing/spawned entities.
            var teleportEntity = new TeleportEntity
            {
                networkedEntityId = entity.NetworkedEntityId,
                targetPosition = entity.WorldTransform,
                yaw = entity.Yaw
            };

            //Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Teleporting {entity.networkedEntityId} for {player.ConnectionId}");
            _server.EnqueueMessage(EMessageTypeClientbound.TeleportEntity, player.ConnectionId, 1, teleportEntity);
        }
        else
        {
            // TODO: Implement destroying entities.
        }
    }

    /// <summary>
    /// Since the entity tracker only works incremental on moved entities, a player spawning in wouldn't see resting
    /// entities. That's why we need to initiate tracking once (and in a batch!) for new players.
    /// </summary>
    /// <param name="player">The player to start tracking for</param>
    public void InitialSpawnForPlayer(Player player)
    {
        foreach (var (id, tty) in _server.EntityService.SpawnedEntities)
        {
            if (!player.NetworkedEntityId.HasValue && id != player.NetworkedEntityId!.Value)
            {
                //Console.WriteLine($"Force updating the tracking for the new player for {tty.networkedEntityId} == {key}");
                UpdateTrackingFor(tty, player);
            }
        }
    }
}