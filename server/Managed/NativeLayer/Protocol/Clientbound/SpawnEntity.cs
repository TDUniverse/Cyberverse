using System.Runtime.InteropServices;
using CyberM.Server.NativeLayer.Protocol.Common;
using CyberM.Server.NativeLayer.Protocol.Serverbound;

namespace CyberM.Server.NativeLayer.Protocol.Clientbound;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct SpawnEntity: IClientBoundPacket
{
    public ulong networkedEntityId;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
    public string recordId;
    public Vector3 spawnPosition;

    public EMessageTypeClientbound GetMessageType() => EMessageTypeClientbound.SpawnEntity;
}