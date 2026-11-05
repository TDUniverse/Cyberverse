using System.Runtime.InteropServices;
using Cyberverse.Server.NativeLayer.Protocol.Serverbound;

namespace Cyberverse.Server.Tests;

public class InitAuthServerBoundCSharpTests
{
    [Fact]
    public void UsernameIsDecodedAsUtf8()
    {
        Assert.Equal("Jürgen", Read("Jürgen"u8.ToArray()).Username);
    }

    [Fact]
    public void UsernameFillingTheBufferHasNoTerminator()
    {
        // The native side copies at most 255 bytes and only zeroes the buffer beforehand, so a full one has no null.
        var name = Enumerable.Repeat((byte)'a', 255).ToArray();
        Assert.Equal(new string('a', 255), Read(name).Username);
    }

    // Builds the struct as the native layer hands it over.
    private static InitAuthServerBoundCSharp Read(byte[] username)
    {
        var type = typeof(InitAuthServerBoundCSharp);
        var cpp = new byte[Marshal.SizeOf(type)];
        // Garbage in the padding after the buffer, as heap memory might hold.
        cpp.AsSpan().Fill((byte)'X');
        var usernameOffset = (int)Marshal.OffsetOf(type, "username");
        cpp.AsSpan(usernameOffset, 255).Clear();
        username.CopyTo(cpp, usernameOffset);
        BitConverter.TryWriteBytes(cpp.AsSpan((int)Marshal.OffsetOf(type, "protocol_version")), 7u);

        var ptr = Marshal.AllocHGlobal(cpp.Length);
        try
        {
            Marshal.Copy(cpp, 0, ptr, cpp.Length);
            var result = Marshal.PtrToStructure<InitAuthServerBoundCSharp>(ptr);
            Assert.Equal(7u, result.protocol_version);
            return result;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
