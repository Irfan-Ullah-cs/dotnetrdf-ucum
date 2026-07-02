using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Fhir.Metrics;

namespace DotNetRdf.Ucum;

internal static class UcumService
{
    private static readonly Lazy<SystemOfUnits> _system = new(UCUM.Load);

    private static readonly Regex _lexicalRegex =
        new(@"^([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)\s+(.+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    internal static (string NumericPart, string UnitPart) SplitLexicalForm(string lexicalForm)
    {
        var match = _lexicalRegex.Match(lexicalForm.Trim());
        if (!match.Success)
            throw new UCUMParseException($"Cannot parse CDT lexical form: '{lexicalForm}'");
        var num = match.Groups[1].Value.Replace("E+", "e+").Replace("E-", "e-").Replace("E", "e");
        return (num, match.Groups[2].Value.Trim());
    }

    internal static UCUMQuantity Canonicalize(string lexicalForm)
    {
        var (num, unit) = SplitLexicalForm(lexicalForm);
        return Canonicalize(num, unit);
    }

    internal static UCUMQuantity Canonicalize(string numericPart, string unitPart)
    {
        try
        {
            Quantity q = _system.Value.Quantity(numericPart, unitPart);
            Quantity canonical = _system.Value.Canonical(q);
            decimal value = canonical.Value.ToDecimal();

            // Fhir.Metrics parses through double internally. A mathematically
            // nonzero input (e.g. "5e-325") can fall below both double's and
            // decimal's smallest representable magnitude and silently collapse
            // to 0 with no exception. Detect that case explicitly: if the
            // canonical result is exactly zero but the original lexical
            // mantissa was not, the value underflowed rather than legitimately
            // being zero, and that must surface as a parse failure rather than
            // a silently wrong quantity.
            if (value == 0m && !IsMathematicallyZero(numericPart))
            {
                throw new UCUMParseException(
                    $"Value '{numericPart}' underflows to zero: magnitude is too small to be represented (min ~1e-28)");
            }

            return new UCUMQuantity(value, canonical.Metric.Symbols);
        }
        catch (OverflowException ex)
        {
            throw new UCUMParseException(
                $"Value '{numericPart}' exceeds the supported numeric range (max ~7.9e28)", ex);
        }
        catch (ArgumentException ex)
        {
            throw new UCUMUnitException($"Invalid or unsupported UCUM unit: '{unitPart}'", ex);
        }
        catch (InvalidCastException ex)
        {
            throw new UCUMUnitException(
                $"Cannot canonicalize unit '{unitPart}': special units with offset conversion formulas are not supported",
                ex);
        }
    }

    // A lexical numeric form is mathematically zero only if every digit in its
    // mantissa is '0' (the exponent is irrelevant: "0e500" is zero, "5e-325" is not,
    // even though the latter underflows once represented in double/decimal).
    private static bool IsMathematicallyZero(string numericPart)
    {
        var mantissa = numericPart.Split('e', 'E')[0];
        foreach (char c in mantissa)
        {
            if (c is >= '1' and <= '9')
                return false;
        }
        return true;
    }

    internal static bool SameDimension(UCUMQuantity a, UCUMQuantity b)
    {
        Quantity qa = ToFhirQuantity(a);
        Quantity qb = ToFhirQuantity(b);
        return Quantity.SameDimension(qa, qb);
    }

    internal static UCUMQuantity MultiplyQuantities(UCUMQuantity a, UCUMQuantity b)
    {
        try
        {
            Quantity qa = ToFhirQuantity(a);
            Quantity qb = ToFhirQuantity(b);
            Quantity result = Quantity.Multiply(qa, qb);
            return new UCUMQuantity(result.Value.ToDecimal(), result.Metric.Symbols);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidCastException)
        {
            throw new UCUMArithmeticException($"Cannot multiply '{a.Unit}' and '{b.Unit}'", ex);
        }
    }

    internal static UCUMQuantity DivideQuantities(UCUMQuantity a, UCUMQuantity b)
    {
        if (b.Value == 0)
            throw new UCUMArithmeticException("Division by zero");
        try
        {
            Quantity qa = ToFhirQuantity(a);
            Quantity qb = ToFhirQuantity(b);
            Quantity result = Quantity.Divide(qa, qb);
            return new UCUMQuantity(result.Value.ToDecimal(), result.Metric.Symbols);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidCastException)
        {
            throw new UCUMArithmeticException($"Cannot divide '{a.Unit}' by '{b.Unit}'", ex);
        }
    }

    internal static void ValidateUnit(string ucumCode)
    {
        try
        {
            _system.Value.Metric(ucumCode.Trim());
        }
        catch (Exception ex)
        {
            throw new UCUMUnitException($"Invalid UCUM unit code: '{ucumCode}'", ex);
        }
    }

    private static Quantity ToFhirQuantity(UCUMQuantity q) =>
        _system.Value.Quantity(q.Value.ToString("G", CultureInfo.InvariantCulture), q.Unit);
}