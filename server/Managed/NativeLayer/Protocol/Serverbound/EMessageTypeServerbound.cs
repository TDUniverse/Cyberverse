namespace Cyberverse.Server.NativeLayer.Protocol.Serverbound;

public enum EMessageTypeServerbound: ushort
{
    InitAuth = 0,
    PlayerJoinWorld = 1,
    PlayerActionTracked = 2,
    PlayerPositionUpdate = 3,
    PlayerSpawnCar = 4,
    PlayerMountCar = 5, // TODO: Implement
    PlayerUnmountCar = 6,
    PlayerEquipItem = 7
}