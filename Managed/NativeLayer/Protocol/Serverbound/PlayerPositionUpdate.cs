﻿using System.Runtime.InteropServices;
using CyberM.Server.NativeLayer.Protocol.Common;

namespace CyberM.Server.NativeLayer.Protocol.Serverbound;


[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct PlayerPositionUpdate
{
    // TODO: uint64_t networkTick (relative to the connect in relative server timestamps, because we probably get those batched and relay those batched)
    public Vector3 worldTransform;
    public float yaw;
}