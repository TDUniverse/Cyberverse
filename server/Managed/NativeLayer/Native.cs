using System.Runtime.InteropServices;
using CyberM.Server.NativeLayer.Protocol.Serverbound;

namespace CyberM.Server;

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
    private const string LibraryName = "CyberM.Server.Native";
    
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
}