using CyberM.Server.Types;

namespace CyberM.Server.Services;

public class PlayerService
{
    public readonly Dictionary<uint, Player> ConnectedPlayers = new();
}