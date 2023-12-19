using System.Diagnostics;
using CyberM.Server.PacketHandling;

namespace CyberM.Server;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting CyberM Server 0.0.1 (c) 2023 MeFisto94");
        var server = new GameServer(1337);
        AddTypicalPacketHandlers(server);
        
        var quit = false;

        while (!quit)
        {
            var sw = new Stopwatch();
            sw.Start();
            server.Update(0.1f);
            sw.Stop();
            //Console.WriteLine($"Update took {sw.ElapsedMilliseconds}ms");
            Thread.Sleep(100);
        }

        Native.destroy_gameserver();
    }

    private static void AddTypicalPacketHandlers(GameServer server)
    {
        // TODO: Store the auth handler somewhere
        new AuthPacketHandler().RegisterOnServer(server);
        new PlayerPacketHandler().RegisterOnServer(server);
    }
}
