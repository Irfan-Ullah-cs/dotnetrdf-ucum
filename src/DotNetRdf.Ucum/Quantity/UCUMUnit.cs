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

    public bool Equals(UCUMUnit other) => Code == other.Code;

    public override bool Equals(object? obj) => obj is UCUMUnit u && Equals(u);

    public override int GetHashCode() => Code.GetHashCode();

    public override string ToString() => Code;
}
