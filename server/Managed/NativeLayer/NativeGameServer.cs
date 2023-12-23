namespace Cyberverse.Server.NativeLayer;

public class NativeGameServer
{
    private static bool _hadSuccessfulNativeInit = false;
    private readonly ushort _listeningPort;
    private readonly IntPtr _nativePtr;
    
    public NativeGameServer(ushort listeningPort)
    {
        if (!_hadSuccessfulNativeInit)
        {
            if (!Native.init_gameserver())
            {
                throw new SystemException("Failed to initialize native library");
            }

            _hadSuccessfulNativeInit = true;
        }

        _listeningPort = listeningPort;
        _nativePtr = Native.server_create(listeningPort);
        Native.server_set_connection_state_changed_cb(_nativePtr, OnConnectionStateChange);
    }

    public virtual void Update(float deltaTime)
    {
        Native.server_update(_nativePtr, deltaTime);
    }

    public void RunBlocking()
    {
        Native.server_run_blocking(_nativePtr);
    }

    public Message PollIncomingMessages()
    {
        return Native.server_poll_message(_nativePtr);
    }

    public void EnqueueMessage(Message message)
    {
        Native.server_enqueue_message(_nativePtr, message);
    }

    protected virtual void OnConnectionStateChange(ESteamNetworkingConnectionState state, uint connectionId)
    {
    }
}