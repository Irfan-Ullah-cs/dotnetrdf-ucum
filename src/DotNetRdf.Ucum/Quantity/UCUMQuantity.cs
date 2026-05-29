using System;

namespace DotNetRdf.Ucum;

public readonly struct UCUMQuantity : IEquatable<UCUMQuantity>, IComparable<UCUMQuantity>
{
    public decimal Value { get; }
    public string Unit { get; }

    public UCUMQuantity(decimal value, string unit)
    {
        Value = value;
        Unit = unit ?? throw new ArgumentNullException(nameof(unit));
    }

    public static UCUMQuantity Parse(string lexicalForm) => UcumService.Canonicalize(lexicalForm);

    public string ToLexicalForm() => $"{Value} {Unit}";

    public bool SameDimension(UCUMQuantity other) => UcumService.SameDimension(this, other);

    public UCUMQuantity Add(UCUMQuantity other)
    {
        if (!SameDimension(other))
            throw new UCUMDimensionException(
                $"Cannot add '{Unit}' and '{other.Unit}': incompatible dimensions");
        return new UCUMQuantity(Value + other.Value, Unit);
    }

    public UCUMQuantity Subtract(UCUMQuantity other)
    {
        if (!SameDimension(other))
            throw new UCUMDimensionException(
                $"Cannot subtract '{other.Unit}' from '{Unit}': incompatible dimensions");
        return new UCUMQuantity(Value - other.Value, Unit);
    }

    public UCUMQuantity Multiply(UCUMQuantity other) =>
        UcumService.MultiplyQuantities(this, other);

    public UCUMQuantity Multiply(decimal scalar) =>
        new(Value * scalar, Unit);

    public UCUMQuantity Divide(UCUMQuantity other) =>
        UcumService.DivideQuantities(this, other);

    public UCUMQuantity Divide(decimal scalar)
    {
        if (scalar == 0)
            throw new UCUMArithmeticException("Division by zero");
        return new UCUMQuantity(Value / scalar, Unit);
    }

    public int CompareTo(UCUMQuantity other)
    {
        if (!SameDimension(other))
            throw new UCUMDimensionException(
                $"Cannot compare '{Unit}' and '{other.Unit}': incompatible dimensions");
        return Value.CompareTo(other.Value);
    }

    public static bool operator <(UCUMQuantity a, UCUMQuantity b) => a.CompareTo(b) < 0;
    public static bool operator >(UCUMQuantity a, UCUMQuantity b) => a.CompareTo(b) > 0;
    public static bool operator <=(UCUMQuantity a, UCUMQuantity b) => a.CompareTo(b) <= 0;
    public static bool operator >=(UCUMQuantity a, UCUMQuantity b) => a.CompareTo(b) >= 0;

    public bool Equals(UCUMQuantity other) => Unit == other.Unit && Value == other.Value;

    public override bool Equals(object? obj) => obj is UCUMQuantity q && Equals(q);

    public override int GetHashCode() => HashCode.Combine(Value, Unit);

    public override string ToString() => ToLexicalForm();
}