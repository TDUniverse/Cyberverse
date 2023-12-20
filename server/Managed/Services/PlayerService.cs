using Cyberverse.Server.Types;

namespace Cyberverse.Server.Services;

public class PlayerService
{
    public readonly Dictionary<uint, Player> ConnectedPlayers = new();
}