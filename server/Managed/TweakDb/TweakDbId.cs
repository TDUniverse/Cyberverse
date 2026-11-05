namespace Cyberverse.Server.TweakDb;

/// <summary>
/// A TweakDB record id: <c>(name length &lt;&lt; 32) | crc32(name)</c>, see <see cref="Types.RecordIdUtils"/>.
/// Converts implicitly from plain ulongs (e.g. packet fields) and from every record enum in
/// <c>Cyberverse.Server.TweakDb.Generated</c> (e.g. <c>Character.Panam</c>), whose conversions are generated in the other
/// part of this struct.
/// </summary>
public readonly partial record struct TweakDbId(ulong Value)
{
    public static implicit operator TweakDbId(ulong value) => new(value);
    public static implicit operator ulong(TweakDbId id) => id.Value;

    public override string ToString() => $"0x{Value:X}";
}
