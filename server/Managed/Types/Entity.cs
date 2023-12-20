using Cyberverse.Server.NativeLayer.Protocol.Common;

namespace Cyberverse.Server.Types;

/// <summary>
/// Don't overuse this class, it's just a simple dataclass, we want to handle complex data in a different way.
///
/// Technically the only thing an entity is is it's entityId, both the recordId (which is the game asset for it and
/// the position are contextually relevant only and could be different maps based on the entityId).
/// </summary>
public class Entity
{
    public readonly ulong NetworkedEntityId;
    /// <summary>
    /// 0 is unowned, could be server controlled.
    /// </summary>
    public ulong NetworkIdOwner;
    public ulong RecordId;
    public Vector3 WorldTransform;
    public float Yaw;

    public Entity(ulong entityId, ulong recordId)
    {
        NetworkedEntityId = entityId;
        RecordId = recordId;
    }
}