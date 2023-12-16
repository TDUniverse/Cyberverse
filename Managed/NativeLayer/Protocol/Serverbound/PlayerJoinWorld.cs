using System.Runtime.InteropServices;
using CyberM.Server.NativeLayer.Protocol.Common;

namespace CyberM.Server.NativeLayer.Protocol.Serverbound;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct PlayerJoinWorld
{
    public Vector3 position;
}