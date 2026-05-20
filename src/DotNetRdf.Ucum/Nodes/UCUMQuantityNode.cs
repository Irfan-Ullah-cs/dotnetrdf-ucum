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

    // Lazily parsed and cached canonical quantity.
    public UCUMQuantity Quantity =>
        _quantity ??= UcumService.Canonicalize(Value);

    // Extracts a UCUMQuantity from any IValuedNode that holds a cdt:ucum literal.
    public static UCUMQuantity ExtractQuantity(IValuedNode node)
    {
        if (node is UCUMQuantityNode qn) return qn.Quantity;
        if (node is ILiteralNode lit && CdtNamespace.IsCdtQuantityDatatype(lit.DataType))
            return UcumService.Canonicalize(lit.Value);
        throw new RdfQueryException(
            $"Cannot extract a UCUM quantity from a node of type {node?.GetType().Name}");
    }

    public string AsString() => Value;

    public long AsInteger() =>
        throw new RdfQueryException("Cannot convert a CDT quantity literal to an integer");

    public decimal AsDecimal() =>
        throw new RdfQueryException("Cannot convert a CDT quantity literal to a decimal");

    public float AsFloat() =>
        throw new RdfQueryException("Cannot convert a CDT quantity literal to a float");

    public double AsDouble() =>
        throw new RdfQueryException("Cannot convert a CDT quantity literal to a double");

    public bool AsBoolean() =>
        throw new RdfQueryException("Cannot convert a CDT quantity literal to a boolean");

    public DateTime AsDateTime() =>
        throw new RdfQueryException("Cannot convert a CDT quantity literal to a DateTime");

    public DateTimeOffset AsDateTimeOffset() =>
        throw new RdfQueryException("Cannot convert a CDT quantity literal to a DateTimeOffset");

    public TimeSpan AsTimeSpan() =>
        throw new RdfQueryException("Cannot convert a CDT quantity literal to a TimeSpan");

    public string EffectiveType => DataType.AbsoluteUri;

    // CDT quantities are not XSD numeric; arithmetic is handled by the CDT operators.
    public SparqlNumericType NumericType => SparqlNumericType.NaN;
}
