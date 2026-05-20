using System;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace DotNetRdf.Ucum;

public class UCUMUnitNode : LiteralNode, IValuedNode
{
    private UCUMUnit? _unit;

    public UCUMUnitNode(string lexicalForm, Uri datatypeUri)
        : base(lexicalForm, datatypeUri, false) { }

    public UCUMUnit Unit => _unit ??= new UCUMUnit(Value);

    public string AsString() => Value;

    public long AsInteger() =>
        throw new RdfQueryException("Cannot convert a CDT unit literal to an integer");

    public decimal AsDecimal() =>
        throw new RdfQueryException("Cannot convert a CDT unit literal to a decimal");

    public float AsFloat() =>
        throw new RdfQueryException("Cannot convert a CDT unit literal to a float");

    public double AsDouble() =>
        throw new RdfQueryException("Cannot convert a CDT unit literal to a double");

    public bool AsBoolean() =>
        throw new RdfQueryException("Cannot convert a CDT unit literal to a boolean");

    public DateTime AsDateTime() =>
        throw new RdfQueryException("Cannot convert a CDT unit literal to a DateTime");

    public DateTimeOffset AsDateTimeOffset() =>
        throw new RdfQueryException("Cannot convert a CDT unit literal to a DateTimeOffset");

    public TimeSpan AsTimeSpan() =>
        throw new RdfQueryException("Cannot convert a CDT unit literal to a TimeSpan");

    public string EffectiveType => DataType.AbsoluteUri;

    public SparqlNumericType NumericType => SparqlNumericType.NaN;
}
