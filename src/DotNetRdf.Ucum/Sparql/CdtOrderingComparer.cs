using System.Globalization;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;

namespace DotNetRdf.Ucum;

public sealed class CdtOrderingComparer : SparqlOrderingComparer
{
    public CdtOrderingComparer()
        : base(CultureInfo.InvariantCulture, CompareOptions.Ordinal) { }

    public override int Compare(INode x, INode y)
    {
        if (x is ILiteralNode lx && CdtNamespace.IsCdtQuantityDatatype(lx.DataType) &&
            y is ILiteralNode ly && CdtNamespace.IsCdtQuantityDatatype(ly.DataType))
        {
            try
            {
                var qx = UcumService.Canonicalize(lx.Value);
                var qy = UcumService.Canonicalize(ly.Value);
                if (!UcumService.SameDimension(qx, qy)) return base.Compare(x, y);
                return qx.CompareTo(qy);
            }
            catch { }
        }
        return base.Compare(x, y);
    }
}
