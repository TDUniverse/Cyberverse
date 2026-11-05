using System.Reflection;
using Cyberverse.Server.TweakDb;
using Cyberverse.Server.TweakDb.Generated;
using Cyberverse.Server.Types;

namespace Cyberverse.Server.Tests;

public class TweakDbTests
{
    private static IEnumerable<Type> Groups => typeof(Character).Assembly.GetTypes()
        .Where(t => t.IsEnum && t.Namespace == typeof(Character).Namespace);

    public static TheoryData<Type> GroupData => new(Groups);

    [Fact]
    public void GeneratedGroupsExist()
    {
        Assert.Contains(typeof(Character), Groups);
        Assert.Contains(typeof(Vehicle), Groups);
    }

    [Theory]
    [MemberData(nameof(GroupData))]
    public void MemberNamesGiveBackTheirRecordId(Type group)
    {
        // The record name is rebuilt as "<Group>.<member>", e.g. Character.Panam, so this also checks the names.
        Assert.Equal(typeof(ulong), Enum.GetUnderlyingType(group));
        foreach (var field in group.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var name = $"{group.Name}.{field.Name}";
            Assert.Equal((name, RecordIdUtils.ToRecordId(name)), (name, (ulong)field.GetRawConstantValue()!));
        }
    }

    [Theory]
    [MemberData(nameof(GroupData))]
    public void GroupConvertsToTweakDbId(Type group)
    {
        var conversion = typeof(TweakDbId).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(m => m.Name == "op_Implicit" && m.GetParameters().Single().ParameterType == group);
        Assert.NotNull(conversion);
    }

    [Fact]
    public void KnownVectors()
    {
        // Computed independently with Python's zlib.crc32
        Assert.Equal(0xFC67F0E01UL, (ulong)Character.Panam);
        Assert.Equal(0x283C959823UL, (ulong)Vehicle.v_sport2_porsche_911turbo_player);
    }

    [Fact]
    public void EnumsAndUlongsConvertToTheSameTweakDbId()
    {
        TweakDbId fromEnum = Character.Panam;
        TweakDbId fromUlong = 0xFC67F0E01UL;
        Assert.Equal(fromUlong, fromEnum);
        Assert.Equal(0xFC67F0E01UL, (ulong)fromEnum);
        Assert.Equal("Character.operator", $"{nameof(Character)}.{Character.@operator}");
    }
}
