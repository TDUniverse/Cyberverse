using System.Runtime.InteropServices;

namespace CyberM.Server.NativeLayer.Protocol.Common;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct Vector3
{
    public float x;
    public float y;
    public float z;
}