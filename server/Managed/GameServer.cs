namespace CyberM.Server;

using NativeLayer.Protocol.Common;
using Types;
using NativeLayer.Protocol.Clientbound;
using NativeLayer.Protocol.Serverbound;

public class GameServer : NativeGameServer
{
    private readonly Dictionary<uint, Player> _connectedPlayers = new();
    private readonly Dictionary<ulong, Entity> _spawnedEntities = new();
    private ulong _entityIdCounter = 0u;
    
    // TODO: proper visibility tracking system.
    private readonly Dictionary<uint, List<ulong>> _trackedEntities = new();
    public GameServer(ushort listeningPort) : base(listeningPort)
    {
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

    protected void EnqueueMessage<T>(EMessageTypeClientbound messageType, uint connectionId, byte channelId, T content) where T: struct
    {
        Message outputMessage = default;
        outputMessage.MessageTypeClientBound(messageType);
        outputMessage.channelId = channelId;
        outputMessage.connectionId = connectionId;
        
        outputMessage.MarshalFrom(content);
        EnqueueMessage(outputMessage);
    }
    
    // TODO: Move somewhere.
    const uint PROTOCOL_VERSION_CURRENT = 0x0;
    
    protected void ProcessMessage(Message message)
    {
        if (message.MessageTypeServerBound() == EMessageTypeServerbound.InitAuth)
        {
            var auth = message.MarshalTo<InitAuthServerbound>();
            // Hint: From here on, data is freed, so we can/have to drop the msg.
            Console.WriteLine($"Connection request from {auth.username}");
            
            AuthResultClientBound resultPacket = default;
            resultPacket.protocol_version = PROTOCOL_VERSION_CURRENT;
            
            if (auth.protocol_version != PROTOCOL_VERSION_CURRENT)
            {
                resultPacket.auth_result = EAuthResult.VersionMismatch;
            }
            else
            {
                //const auto validation = m_authController.ValidateUser(init_auth->username);
                var validation = true;
                resultPacket.auth_result = validation ? EAuthResult.Ok : EAuthResult.ValidationFailed;
                _connectedPlayers.Add(message.connectionId, new Player { Name = auth.username, ConnectionId = message.connectionId });
            }

            Message outputMessage = default;
            outputMessage.channelId = 0;
            outputMessage.MessageTypeClientBound(EMessageTypeClientbound.InitAuthResult);
            outputMessage.connectionId = message.connectionId;
            
            outputMessage.MarshalFrom(resultPacket);
            EnqueueMessage(outputMessage);
        } else if (message.messageType == 1)
        {
            var playerJoin = message.MarshalTo<PlayerJoinWorld>();
            if (_connectedPlayers.TryGetValue(message.connectionId, out var player))
            {
                Console.WriteLine("Player {0} joined the world at ({1}, {2}, {3})", player.Name, 
                    playerJoin.position.x, playerJoin.position.y, playerJoin.position.z);
                var entity = DoSpawnEntity();
                player.NetworkedEntityId = entity.networkedEntityId;
                entity.recordId = new Random().NextSingle() > 0.5f ? "Character.Judy" : "Character.Panam";
                entity.worldTransform = playerJoin.position;
                
                // Since we only call the tracking for moved entities, force sync every entity to that player:
                foreach (var (key, tty) in _spawnedEntities)
                {
                    if (tty.networkedEntityId != entity.networkedEntityId)
                    {
                        //Console.WriteLine($"Force updating the tracking for the new player for {tty.networkedEntityId} == {key}");
                        UpdateTrackingFor(tty, player);
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error: Could not find player with id {message.connectionId} in the player list.");
            }
        } else if (message.messageType == 2)
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
        } else if (message.messageType == 3)
        {
            var positionUpdate = message.MarshalTo<PlayerPositionUpdate>();
            
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

            if (_connectedPlayers.TryGetValue(message.connectionId, out var player))
            {
                if (_spawnedEntities.TryGetValue(player.NetworkedEntityId, out var entity))
                {
                    // TODO: sometimes the game sends faulty updates to ~0, 3.16, 0.
                    //Console.WriteLine($"Move {player.Name} ({player.ConnectionId} / {message.connectionId}) to ({positionUpdate.worldTransform.x}, {positionUpdate.worldTransform.y}, {positionUpdate.worldTransform.z})");

                    // TODO: This is happening before the savegame has loaded. A better solution is to prevent updating networkEntityIds that are not spawned yet,
                    // because what happens here is that player.NetworkedEntityId is 0 because it has no entity yet, thus moving the old player.
                    if (positionUpdate.worldTransform.z == 0 && positionUpdate.worldTransform.x == 0.010673523f)
                    {
                        Console.WriteLine("Skipping position update");
                        return;
                    }
                    
                    entity.worldTransform = positionUpdate.worldTransform;
                    entity.yaw = positionUpdate.yaw;
                    UpdateTrackingFor(entity);
                }
            }
        }
    }

    protected Entity DoSpawnEntity()
    {
        var id = _entityIdCounter++;
        var entity = new Entity { networkedEntityId = id };
        _spawnedEntities.Add(id, entity);
        return entity;
    }

    protected void OnStartTrackingEntity(Player player, Entity entity)
    {
        if (!_trackedEntities.ContainsKey(player.ConnectionId))
        {
            _trackedEntities.Add(player.ConnectionId, new List<ulong>());
        }

        if (!_trackedEntities[player.ConnectionId].Contains(entity.networkedEntityId))
        {
            _trackedEntities[player.ConnectionId].Add(entity.networkedEntityId);
            var spawnEntity = new SpawnEntity
            {
                networkedEntityId = entity.networkedEntityId,
                recordId = entity.recordId,
                spawnPosition = entity.worldTransform
            };
            
            EnqueueMessage(EMessageTypeClientbound.SpawnEntity, player.ConnectionId, 1, spawnEntity);
        }
    }

    protected void UpdateTrackingFor(Entity entity)
    {
        //Console.WriteLine($"Update tracking for {entity.networkedEntityId}");

        foreach (var (connectionId, player) in _connectedPlayers)
        {
            // TODO: When we only update the tracking for one 
            // Don't track yourself.
            if (player.NetworkedEntityId != entity.networkedEntityId)
            {
                UpdateTrackingFor(entity, player);
            }
        }
    }

    private void UpdateTrackingFor(Entity entity, Player player)
    {
        var connectionId = player.ConnectionId;
        
        //Console.WriteLine($"For player {player.Name}");
        // TODO: Check for out of bounds.
        var playerPosition = _spawnedEntities[player.NetworkedEntityId].worldTransform;
        if (DistanceSquared(playerPosition, entity.worldTransform) <= 100 * 100)
        {
            //Console.WriteLine("Inside range");
                
            if (!_trackedEntities.ContainsKey(connectionId))
            {
                _trackedEntities.Add(connectionId, new List<ulong>());
            }
                
            if (!_trackedEntities[connectionId].Contains(entity.networkedEntityId))
            {
                OnStartTrackingEntity(player, entity);
            }
                
            // TODO: In the future, we may not want to immediately update entities that have been recently spawned,
            //  because if spawning and updating are different channels, update may arrive before spawning.
            //  Though, the client should ignore updates for non existing/spawned entities.
            var teleportEntity = new TeleportEntity
            {
                networkedEntityId = entity.networkedEntityId,
                targetPosition = entity.worldTransform,
                yaw = entity.yaw
            };
            
            //Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Teleporting {entity.networkedEntityId} for {player.ConnectionId}");
            EnqueueMessage(EMessageTypeClientbound.TeleportEntity, player.ConnectionId, 1, teleportEntity);
        }
    }

    private static float DistanceSquared(Vector3 a, Vector3 b)
    {
        return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
    }
}