using System.Runtime.InteropServices;
using Cyberverse.Server.NativeLayer.Protocol.Clientbound;
using Cyberverse.Server.NativeLayer.Protocol.Serverbound;

namespace Cyberverse.Server.NativeLayer;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct Message
{
    public byte channelId;
    public ushort messageType;
    public uint connectionId;
    public IntPtr data;

    public EMessageTypeServerbound MessageTypeServerBound()
    {
        return (EMessageTypeServerbound)messageType;
    }
    
    public void MessageTypeClientBound(EMessageTypeClientbound type)
    {
        messageType = (ushort)type;
    }

    public bool HasData()
    {
        return data != IntPtr.Zero;
    }

    // Call this before dropping _this_
    public void ReleaseNative()
    {
        if (!HasData())
        {
            throw new InvalidOperationException("Cannot release non-existent data!");
        }
        
        Native.message_data_release(data);
        data = IntPtr.Zero;
    }

    public T MarshalTo<T>() where T: struct
    {
        if (!HasData())
        {
            throw new InvalidOperationException("Cannot marshal a message that holds no data (anymore)!");
        }
        
        var result = Marshal.PtrToStructure<T>(data);
        ReleaseNative();
        return result;
    }

    public void MarshalFrom<T>(T content) where T : struct
    {
        if (HasData())
        {
            throw new InvalidOperationException("Cannot marshal into a message, that already holds data");
        }

        data = Native.message_data_allocate((ulong)Marshal.SizeOf<T>());
        Marshal.StructureToPtr(content, data, false);
    }
}

public static class Native
{
    private const string LibraryName = "Cyberverse.Server.Native";
    
    [DllImport(LibraryName)]
    public static extern bool init_gameserver();
    
    [DllImport(LibraryName)]
    public static extern void destroy_gameserver();

    [DllImport(LibraryName)]
    public static extern IntPtr server_create(ushort serverPort);

    [DllImport(LibraryName)]
    public static extern void server_run_blocking(IntPtr server);
    
    [DllImport(LibraryName)]
    public static extern void server_update(IntPtr server, float deltaTime);
    
    [DllImport(LibraryName)]
    public static extern void message_data_release(IntPtr data);

    [DllImport(LibraryName)]
    public static extern IntPtr message_data_allocate(ulong size);
    
    [DllImport(LibraryName)]
    public static extern Message server_poll_message(IntPtr server);
    
    [DllImport(LibraryName)]
    public static extern void server_enqueue_message(IntPtr server, Message message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ConnectionStateChangedCallback(ESteamNetworkingConnectionState state, uint connectionId);
    
    [DllImport(LibraryName)]
    public static extern void server_set_connection_state_changed_cb(IntPtr server,
        [MarshalAs(UnmanagedType.FunctionPtr)] ConnectionStateChangedCallback callback);
}

public enum ESteamNetworkingConnectionState: uint
{
    /// We are trying to establish whether peers can talk to each other,
	/// whether they WANT to talk to each other, perform basic auth,
	/// and exchange crypt keys.
	///
	/// - For connections on the "client" side (initiated locally):
	///   We're in the process of trying to establish a connection.
	///   Depending on the connection type, we might not know who they are.
	///   Note that it is not possible to tell if we are waiting on the
	///   network to complete handshake packets, or for the application layer
	///   to accept the connection.
	///
	/// - For connections on the "server" side (accepted through listen socket):
	///   We have completed some basic handshake and the client has presented
	///   some proof of identity.  The connection is ready to be accepted
	///   using AcceptConnection().
	///
	/// In either case, any unreliable packets sent now are almost certain
	/// to be dropped.  Attempts to receive packets are guaranteed to fail.
	/// You may send messages if the send mode allows for them to be queued.
	/// but if you close the connection before the connection is actually
	/// established, any queued messages will be discarded immediately.
	/// (We will not attempt to flush the queue and confirm delivery to the
	/// remote host, which ordinarily happens when a connection is closed.)
	Connecting = 1,

	/// Some connection types use a back channel or trusted 3rd party
	/// for earliest communication.  If the server accepts the connection,
	/// then these connections switch into the rendezvous state.  During this
	/// state, we still have not yet established an end-to-end route (through
	/// the relay network), and so if you send any messages unreliable, they
	/// are going to be discarded.
	FindingRoute = 2,

	/// We've received communications from our peer (and we know
	/// who they are) and are all good.  If you close the connection now,
	/// we will make our best effort to flush out any reliable sent data that
	/// has not been acknowledged by the peer.  (But note that this happens
	/// from within the application process, so unlike a TCP connection, you are
	/// not totally handing it off to the operating system to deal with it.)
	Connected = 3,

	/// Connection has been closed by our peer, but not closed locally.
	/// The connection still exists from an API perspective.  You must close the
	/// handle to free up resources.  If there are any messages in the inbound queue,
	/// you may retrieve them.  Otherwise, nothing may be done with the connection
	/// except to close it.
	///
	/// This stats is similar to CLOSE_WAIT in the TCP state machine.
	ClosedByPeer = 4,

	/// A disruption in the connection has been detected locally.  (E.g. timeout,
	/// local internet connection disrupted, etc.)
	///
	/// The connection still exists from an API perspective.  You must close the
	/// handle to free up resources.
	///
	/// Attempts to send further messages will fail.  Any remaining received messages
	/// in the queue are available.
	ProblemDetectedLocally = 5,
}