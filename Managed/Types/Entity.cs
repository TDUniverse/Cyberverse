using CyberM.Server.NativeLayer.Protocol.Common;

namespace CyberM.Server.Types;

/// <summary>
/// Don't overuse this class, it's just a simple dataclass, we want to handle complex data in a different way
/// </summary>
public class Entity
{
    public ulong networkedEntityId;
    public string recordId;
    public Vector3 worldTransform;
    public float yaw;
}