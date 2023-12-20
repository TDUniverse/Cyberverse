using System.Runtime.InteropServices;
using Cyberverse.Server.NativeLayer.Protocol.Common;

namespace Cyberverse.Server.NativeLayer.Protocol.Serverbound;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct PlayerActionTracked
{
    // TODO: bool buttonState? (We could trigger on press or release)
    // TODO: uint64_t networkTick (relative to the connect in relative server timestamps, because we probably get those batched and relay those batched)
    public EPlayerAction action;
    public Vector3 worldTransform;
}