using System.Diagnostics;
using Cyberverse.Server.NativeLayer;
using Cyberverse.Server.PacketHandling;
using NLog;

namespace Cyberverse.Server;

public class Program
{
    static void Main(string[] args)
    {
        InitLogging();
        LogManager.GetCurrentClassLogger().Info("Starting Cyberverse Server 0.0.1 (c) 2023-2025 MeFisto94");
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
    
    private static void InitLogging()
    {
        NLog.Config.LoggingConfiguration config = new NLog.Config.LoggingConfiguration();
        var logFile = new NLog.Targets.FileTarget("logfile")
        {
            CreateDirs = true,
            FileName = "logs/log.txt",
            ArchiveOldFileOnStartup = true,
            ArchiveEvery = NLog.Targets.FileArchivePeriod.Hour,
            ArchiveAboveSize = 100000000,
            EnableArchiveFileCompression = true,
            MaxArchiveDays = 30
        };
        var logConsole = new NLog.Targets.ColoredConsoleTarget("logconsole")
        {
            EnableAnsiOutput = true, // Maybe this improves the Open Game Panel(?)
            DetectOutputRedirected = true // Disables colors when a file is detected
        };

        #if DEBUG
        config.AddRule(LogLevel.Trace, LogLevel.Fatal, logConsole);
        #else
        config.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
        #endif
        config.AddRule(LogLevel.Warn, LogLevel.Fatal, logFile);

        LogManager.Configuration = config;
    }
}
