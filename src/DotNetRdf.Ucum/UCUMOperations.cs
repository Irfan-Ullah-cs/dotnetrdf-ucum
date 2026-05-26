using VDS.RDF;

namespace DotNetRdf.Ucum;

/// <summary>
/// Static API for manipulating CDT quantity and unit literals outside of SPARQL evaluation.
/// </summary>
public static class UCUMOperations
{
    /// <summary>Returns the canonical decimal value of a cdt:ucum literal.</summary>
    public static decimal GetValue(ILiteralNode node) =>
        UcumService.Canonicalize(node.Value).Value;

    /// <summary>Returns the canonical SI unit string of a cdt:ucum literal.</summary>
    public static string GetUnit(ILiteralNode node) =>
        UcumService.Canonicalize(node.Value).Unit;

    /// <summary>Adds two cdt:ucum literals. Both must have the same physical dimension.</summary>
    public static UCUMQuantityNode Add(ILiteralNode a, ILiteralNode b)
    {
        var result = UcumService.Canonicalize(a.Value).Add(UcumService.Canonicalize(b.Value));
        return new UCUMQuantityNode(result.ToLexicalForm(), CdtNamespace.UcumUri);
    }

    /// <summary>Subtracts two cdt:ucum literals. Both must have the same physical dimension.</summary>
    public static UCUMQuantityNode Subtract(ILiteralNode a, ILiteralNode b)
    {
        var result = UcumService.Canonicalize(a.Value).Subtract(UcumService.Canonicalize(b.Value));
        return new UCUMQuantityNode(result.ToLexicalForm(), CdtNamespace.UcumUri);
    }

    /// <summary>Multiplies two cdt:ucum literals.</summary>
    public static UCUMQuantityNode Multiply(ILiteralNode a, ILiteralNode b)
    {
        var result = UcumService.Canonicalize(a.Value).Multiply(UcumService.Canonicalize(b.Value));
        return new UCUMQuantityNode(result.ToLexicalForm(), CdtNamespace.UcumUri);
    }

    /// <summary>Divides one cdt:ucum literal by another.</summary>
    public static UCUMQuantityNode Divide(ILiteralNode a, ILiteralNode b)
    {
        var result = UcumService.Canonicalize(a.Value).Divide(UcumService.Canonicalize(b.Value));
        return new UCUMQuantityNode(result.ToLexicalForm(), CdtNamespace.UcumUri);
    }

    /// <summary>Compares two cdt:ucum literals. Returns negative, zero, or positive.</summary>
    public static int Compare(ILiteralNode a, ILiteralNode b) =>
        UcumService.Canonicalize(a.Value).CompareTo(UcumService.Canonicalize(b.Value));

    /// <summary>Returns true if two cdt:ucum literals have equal values regardless of unit. Returns false if dimensions are incompatible.</summary>
    public static bool Equals(ILiteralNode a, ILiteralNode b)
    {
        try { return Compare(a, b) == 0; }
        catch (UCUMDimensionException) { return false; }
    }

    /// <summary>Returns true if two cdt:ucum literals measure the same physical dimension.</summary>
    public static bool SameDimension(ILiteralNode a, ILiteralNode b) =>
        UcumService.Canonicalize(a.Value).SameDimension(UcumService.Canonicalize(b.Value));

    /// <summary>Returns the canonical lexical form of a cdt:ucum literal.</summary>
    public static string Canonicalize(ILiteralNode node) =>
        UcumService.Canonicalize(node.Value).ToLexicalForm();
}