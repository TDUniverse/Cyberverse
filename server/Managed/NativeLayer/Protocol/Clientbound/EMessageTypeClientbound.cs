namespace Cyberverse.Server.NativeLayer.Protocol.Clientbound;

public enum EMessageTypeClientbound: ushort
{
    InitAuthResult = 0,
    SpawnEntity = 1,
    TeleportEntity = 2,
    DestroyEntity = 3
}