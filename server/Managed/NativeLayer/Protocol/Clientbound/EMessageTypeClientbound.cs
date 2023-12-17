namespace CyberM.Server.NativeLayer.Protocol.Serverbound;

public enum EMessageTypeClientbound: ushort
{
    InitAuthResult = 0,
    SpawnEntity = 1,
    TeleportEntity = 2,
}