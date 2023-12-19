namespace CyberM.Server.Types;

/// <summary>
/// Don't overuse this class, it's just a simple dataclass, we want to handle complex data in a different way
/// </summary>
public class Player
{
    // TODO: Validate by JWT claim
    public string? Name;

    /// <summary>
    /// Nullable: If spawned or not (because 0 could be a valid entityId!)
    /// </summary>
    // TODO: Revert this ASAP and use 0 as invalid entity id convention
    public ulong? NetworkedEntityId = null;
    public uint ConnectionId;
}