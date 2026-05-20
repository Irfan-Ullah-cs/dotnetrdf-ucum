using System.Globalization;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace DotNetRdf.Ucum;

// Wraps the default ISparqlNodeComparer. CDT literals are compared numerically
// after canonicalization; all other nodes are delegated to the inner comparer.
public sealed class CdtNodeComparer : ISparqlNodeComparer
{
    private readonly ISparqlNodeComparer _inner;

    public CdtNodeComparer(ISparqlNodeComparer inner)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    public CultureInfo Culture => _inner.Culture;
    public CompareOptions Options => _inner.Options;

    public bool TryCompare(INode x, INode y, out int result)
    {
        if (TryCompareCdt(x, y, out result)) return true;
        return _inner.TryCompare(x, y, out result);
    }

    public bool TryCompare(IValuedNode x, IValuedNode y, out int result)
    {
        if (TryCompareCdt(x, y, out result)) return true;
        return _inner.TryCompare(x, y, out result);
    }

    private static bool TryCompareCdt(INode? x, INode? y, out int result)
    {
        result = 0;
        if (x is not ILiteralNode lx || y is not ILiteralNode ly) return false;
        if (!CdtNamespace.IsCdtQuantityDatatype(lx.DataType)) return false;
        if (!CdtNamespace.IsCdtQuantityDatatype(ly.DataType)) return false;

        try
        {
            UCUMQuantity qx = UcumService.Canonicalize(lx.Value);
            UCUMQuantity qy = UcumService.Canonicalize(ly.Value);

            // Quantities of incompatible dimensions return false (no ordering defined).
            if (!qx.SameDimension(qy)) return false;

            result = qx.CompareTo(qy);
            return true;
        }
        catch (UCUMException)
        {
            return false;
        }
    }
}
