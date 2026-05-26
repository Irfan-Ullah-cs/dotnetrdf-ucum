using System;

namespace DotNetRdf.Ucum;

public readonly struct UCUMUnit : IEquatable<UCUMUnit>
{
    public string Code { get; }

    public UCUMUnit(string code)
    {
        Code = code?.Trim() ?? throw new ArgumentNullException(nameof(code));
    }

    public static UCUMUnit Parse(string lexicalForm)
    {
        var code = lexicalForm?.Trim()
            ?? throw new UCUMParseException("Lexical form cannot be null");
        UcumService.ValidateUnit(code);
        return new UCUMUnit(code);
    }

    public string ToLexicalForm() => Code;

    public bool Equals(UCUMUnit other)
    {
        if (Code == other.Code) return true;
        try
        {
            var a = UcumService.Canonicalize("1 " + Code);
            var b = UcumService.Canonicalize("1 " + other.Code);
            return a.Unit == b.Unit && a.Value == b.Value;
        }
        catch
        {
            return false;
        }
    }

    public override bool Equals(object? obj) => obj is UCUMUnit u && Equals(u);

    public override int GetHashCode()
    {
        try
        {
            var canonical = UcumService.Canonicalize("1 " + Code);
            return HashCode.Combine(canonical.Value, canonical.Unit);
        }
        catch
        {
            return Code.GetHashCode();
        }
    }

    public override string ToString() => Code;
}