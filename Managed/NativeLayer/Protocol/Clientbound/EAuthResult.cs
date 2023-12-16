namespace CyberM.Server.NativeLayer.Protocol.Clientbound;

public enum EAuthResult: byte {
    Ok,
    VersionMismatch,
    ValidationFailed // From wrong "password" to being banned.
}