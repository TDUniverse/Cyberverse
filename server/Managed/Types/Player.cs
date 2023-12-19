namespace CyberM.Server.Types;

/// <summary>
/// Don't overuse this class, it's just a simple dataclass, we want to handle complex data in a different way
/// </summary>
public class Player
{
    // TODO: Validate by JWT claim
    public string? Name;
    public uint ConnectionId;
}