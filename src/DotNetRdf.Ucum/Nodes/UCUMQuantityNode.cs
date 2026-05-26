using System;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace DotNetRdf.Ucum;

public class UCUMQuantityNode : LiteralNode, IValuedNode
{
    private UCUMQuantity? _quantity;

    public UCUMQuantityNode(string lexicalForm, Uri datatypeUri)
        : base(lexicalForm, datatypeUri, false) { }

    public UCUMQuantity Quantity => _quantity ??= UcumService.Canonicalize(Value);

    public override int CompareTo(INode other)
    {
        if (other is ILiteralNode lit && CdtNamespace.IsCdtQuantityDatatype(lit.DataType))
        {
            try
            {
                UCUMQuantity q = UcumService.Canonicalize(lit.Value);
                if (!UcumService.SameDimension(Quantity, q)) return base.CompareTo(other);
                return Quantity.CompareTo(q);
            }
            catch { }
        }
        return base.CompareTo(other);
    }

    public static UCUMQuantity ExtractQuantity(IValuedNode node)
    {
        if (node is UCUMQuantityNode qn) return qn.Quantity;
        if (node is ILiteralNode lit && CdtNamespace.IsCdtQuantityDatatype(lit.DataType))
            return UcumService.Canonicalize(lit.Value);
        throw new RdfQueryException(
            $"Cannot extract a UCUM quantity from a node of type {node?.GetType().Name}");
    }

    public string AsString() => Value;

    public long AsInteger() => (long)Quantity.Value;

    public decimal AsDecimal() => Quantity.Value;

    public float AsFloat() => (float)Quantity.Value;

    public double AsDouble() => (double)Quantity.Value;

    public bool AsBoolean() =>
        throw new RdfQueryException("Cannot convert a CDT quantity literal to a boolean");

    public DateTime AsDateTime() =>
        throw new RdfQueryException("Cannot convert a CDT quantity literal to a DateTime");

    public DateTimeOffset AsDateTimeOffset() =>
        throw new RdfQueryException("Cannot convert a CDT quantity literal to a DateTimeOffset");

    public TimeSpan AsTimeSpan() =>
        throw new RdfQueryException("Cannot convert a CDT quantity literal to a TimeSpan");

    public string EffectiveType => DataType.AbsoluteUri;

    public SparqlNumericType NumericType => SparqlNumericType.Decimal;
}