namespace CyberM.Server.NativeLayer.Protocol.Serverbound;

public enum EMessageTypeServerbound: ushort
{
    InitAuth = 0,
    PlayerJoinWorld = 1,
    PlayerActionTracked = 2,
    PlayerPositionUpdate = 3,
    PlayerSpawnCar = 4,
}