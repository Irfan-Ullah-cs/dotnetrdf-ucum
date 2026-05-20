using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using Fhir.Metrics;

namespace DotNetRdf.Ucum;

// All Fhir.Metrics calls are isolated here. No other file references Fhir.Metrics directly.
internal static class UcumService
{
    private static readonly Lazy<SystemOfUnits> _system = new(UCUM.Load);

    // Canonical symbol cache avoids repeated canonicalization of the same unit string.
    private static readonly ConcurrentDictionary<string, string> _canonicalSymbolCache = new();

    // CDT lexical form: a number followed by whitespace and a UCUM unit expression.
    private static readonly Regex _lexicalRegex =
        new(@"^([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)\s+(.+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    internal static (string NumericPart, string UnitPart) SplitLexicalForm(string lexicalForm)
    {
        var match = _lexicalRegex.Match(lexicalForm.Trim());
        if (!match.Success)
            throw new UCUMParseException($"Cannot parse CDT lexical form: '{lexicalForm}'");
        // Fhir.Metrics Exponential only accepts lowercase 'e' in scientific notation.
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
            return new UCUMQuantity(canonical.Value.ToDecimal(), canonical.Metric.Symbols);
        }
        catch (ArgumentException ex)
        {
            throw new UCUMUnitException($"Invalid or unsupported UCUM unit: '{unitPart}'", ex);
        }
        catch (InvalidCastException ex)
        {
            // Celsius, Fahrenheit and other offset-formula units cannot be canonicalized.
            throw new UCUMUnitException(
                $"Cannot canonicalize unit '{unitPart}': special units with offset conversion formulas are not supported",
                ex);
        }
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
