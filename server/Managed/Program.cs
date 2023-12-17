using System.Diagnostics;

namespace CyberM.Server;

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting CyberM Server 0.0.1 (c) 2023 MeFisto94");
        var server = new GameServer(1337);
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
}
