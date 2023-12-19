using System.Runtime.InteropServices;

namespace CyberM.Server.NativeLayer.Protocol.Common;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct Vector3
{
    public float x;
    public float y;
    public float z;
    
    // TODO: would we rather want to cast into System.Numerics? At least for more complicated operations we should!
    public float DistanceSquared(Vector3 b)
    {
        return (x - b.x) * (x - b.x) + (y - b.y) * (y - b.y) + (z - b.z) * (z - b.z);
    }

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
}