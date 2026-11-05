using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Cyberverse.Server.NativeLayer;

namespace Cyberverse.Server.Tests;

/// <summary>
/// The native layer hands the structs in Cyberverse.Server.NativeLayer to and from C# as raw memory, so their marshaled
/// layout has to match the C++ struct of the same name in shared/protocol or server/Native/src/api/message.h. The C++
/// layouts come from clang (which has to be on the PATH), compiled for this machine's platform, so nothing here is
/// maintained by hand. Field names are compared ignoring case, so C# can keep its own naming.
/// </summary>
public partial class ProtocolLayoutTests
{
    private static IEnumerable<Type> ProtocolStructs => typeof(Message).Assembly.GetTypes()
        .Where(t => t is { IsValueType: true, IsEnum: false, IsNested: false, Namespace: not null }
                    && t.Namespace.StartsWith("Cyberverse.Server.NativeLayer")
                    && !t.IsDefined(typeof(CompilerGeneratedAttribute)))
        .OrderBy(t => t.Name);

    public static TheoryData<Type> Structs => new(ProtocolStructs);

    [Theory]
    [MemberData(nameof(Structs))]
    public void MarshaledLayoutMatchesCpp(Type type)
    {
        Assert.True(CppLayouts.Value.TryGetValue(type.Name, out var cpp), $"clang did not dump struct {type.Name}");

        // Sorted (offset, name) pairs: catches fields missing on either side as well as moved ones.
        var expected = cpp.Fields
            .Select(f => (f.Value, f.Key.ToLowerInvariant()))
            .Order();
        var actual = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(f => ((int)Marshal.OffsetOf(type, f.Name), f.Name.ToLowerInvariant()))
            .Order();
        Assert.Equal(expected, actual);
        Assert.Equal(cpp.Size, Marshal.SizeOf(type));
    }

    private static readonly Lazy<Dictionary<string, (int Size, Dictionary<string, int> Fields)>> CppLayouts =
        new(DumpCppLayouts);

    /// <summary>
    /// Compiles a file that includes every protocol header and takes the sizeof of each C# struct's C++ counterpart,
    /// which makes clang dump their layouts. (-fdump-record-layouts-complete would dump all of them without the sizeofs,
    /// but crashes clang 18 on zpp_bits.)
    /// </summary>
    private static Dictionary<string, (int Size, Dictionary<string, int> Fields)> DumpCppLayouts()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (!Directory.Exists(Path.Combine(root.FullName, "shared", "protocol")))
        {
            root = root.Parent ?? throw new DirectoryNotFoundException("Repository root with shared/protocol not found");
        }

        var headers = Directory.GetFiles(Path.Combine(root.FullName, "shared", "protocol"), "*.h", SearchOption.AllDirectories)
            .Order()
            .Append(Path.Combine(root.FullName, "server", "Native", "src", "api", "message.h"));
        var source = new StringBuilder();
        foreach (var header in headers)
        {
            source.AppendLine($"#include \"{header.Replace('\\', '/')}\"");
        }
        foreach (var (type, i) in ProtocolStructs.Select((type, i) => (type, i)))
        {
            source.AppendLine($"constexpr auto size{i} = sizeof({type.Name});");
        }

        var file = Path.Combine(Path.GetTempPath(), $"cyberverse-layouts-{Guid.NewGuid():N}.cpp");
        File.WriteAllText(file, source.ToString());
        try
        {
            var zppBits = Path.Combine(root.FullName, "server", "Native", "deps", "zpp_bits");
            var start = new ProcessStartInfo("clang++")
            {
                ArgumentList = { "-std=c++20", "-fsyntax-only", "-w", "-I", zppBits, "-Xclang", "-fdump-record-layouts", file },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            Process clang;
            try
            {
                clang = Process.Start(start)!;
            }
            catch (System.ComponentModel.Win32Exception e)
            {
                throw new InvalidOperationException("clang++ has to be on the PATH to read the C++ struct layouts", e);
            }
            using var _ = clang;
            var stdout = clang.StandardOutput.ReadToEndAsync();
            var stderr = clang.StandardError.ReadToEndAsync();
            clang.WaitForExit();
            if (clang.ExitCode != 0)
            {
                throw new InvalidOperationException($"clang++ failed ({clang.ExitCode}):\n{stderr.Result}");
            }
            return ParseLayouts(stdout.Result);
        }
        finally
        {
            File.Delete(file);
        }
    }

    // Each dump starts with "  0 | struct Name", lists direct members as "  <offset> |   <type> <name>" (nested members
    // are indented further) and ends with "    | [sizeof=N, ...".
    private static Dictionary<string, (int Size, Dictionary<string, int> Fields)> ParseLayouts(string dump)
    {
        var layouts = new Dictionary<string, (int, Dictionary<string, int>)>();
        string? name = null;
        var fields = new Dictionary<string, int>();
        foreach (var line in dump.Split('\n'))
        {
            if (RecordLine().Match(line) is { Success: true } record)
            {
                name = record.Groups["name"].Value;
                fields = [];
            }
            else if (name != null && FieldLine().Match(line) is { Success: true } field)
            {
                fields[field.Groups["name"].Value] = int.Parse(field.Groups["offset"].Value);
            }
            else if (name != null && SizeLine().Match(line) is { Success: true } size)
            {
                layouts.TryAdd(name, (int.Parse(size.Groups["size"].Value), fields));
                name = null;
            }
        }
        return layouts;
    }

    [GeneratedRegex(@"^\s*0 \| (?:struct|class|union) (?<name>[\w:<>, ]+?)(?: \(empty\))?\r?$")]
    private static partial Regex RecordLine();

    [GeneratedRegex(@"^\s*(?<offset>\d+) \|   \S.* (?<name>\w+)\r?$")]
    private static partial Regex FieldLine();

    [GeneratedRegex(@"^\s*\| \[sizeof=(?<size>\d+),")]
    private static partial Regex SizeLine();
}
