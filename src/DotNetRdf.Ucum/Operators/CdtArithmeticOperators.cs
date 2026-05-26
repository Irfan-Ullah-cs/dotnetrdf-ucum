using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Operators;

namespace DotNetRdf.Ucum;

public sealed class CdtAdditionOperator : BaseOperator
{
    public override SparqlOperatorType Operator => SparqlOperatorType.Add;
    public override bool IsExtension => true;

    public override bool IsApplicable(params IValuedNode[] ns) =>
        ns.Length == 2
        && ns[0] is ILiteralNode l0 && CdtNamespace.IsCdtQuantityDatatype(l0.DataType)
        && ns[1] is ILiteralNode l1 && CdtNamespace.IsCdtQuantityDatatype(l1.DataType);

    public override IValuedNode Apply(params IValuedNode[] ns)
    {
        try
        {
            UCUMQuantity a = UCUMQuantityNode.ExtractQuantity(ns[0]);
            UCUMQuantity b = UCUMQuantityNode.ExtractQuantity(ns[1]);
            UCUMQuantity result = a.Add(b);
            return new UCUMQuantityNode(result.ToLexicalForm(), CdtNamespace.UcumUri);
        }
        catch (UCUMException ex)
        {
            throw new RdfQueryException(ex.Message, ex);
        }
    }
}

public sealed class CdtSubtractionOperator : BaseOperator
{
    public override SparqlOperatorType Operator => SparqlOperatorType.Subtract;
    public override bool IsExtension => true;

    public override bool IsApplicable(params IValuedNode[] ns) =>
        ns.Length == 2
        && ns[0] is ILiteralNode l0 && CdtNamespace.IsCdtQuantityDatatype(l0.DataType)
        && ns[1] is ILiteralNode l1 && CdtNamespace.IsCdtQuantityDatatype(l1.DataType);

    public override IValuedNode Apply(params IValuedNode[] ns)
    {
        try
        {
            UCUMQuantity a = UCUMQuantityNode.ExtractQuantity(ns[0]);
            UCUMQuantity b = UCUMQuantityNode.ExtractQuantity(ns[1]);
            UCUMQuantity result = a.Subtract(b);
            return new UCUMQuantityNode(result.ToLexicalForm(), CdtNamespace.UcumUri);
        }
        catch (UCUMException ex)
        {
            throw new RdfQueryException(ex.Message, ex);
        }
    }
}

public sealed class CdtMultiplicationOperator : BaseOperator
{
    public override SparqlOperatorType Operator => SparqlOperatorType.Multiply;
    public override bool IsExtension => true;

    public override bool IsApplicable(params IValuedNode[] ns)
    {
        if (ns.Length != 2) return false;
        bool leftIsCdt = ns[0] is ILiteralNode l0 && CdtNamespace.IsCdtQuantityDatatype(l0.DataType);
        bool rightIsCdt = ns[1] is ILiteralNode l1 && CdtNamespace.IsCdtQuantityDatatype(l1.DataType);
        bool rightIsNumeric = ns[1] != null && ns[1].NumericType != SparqlNumericType.NaN;
        return leftIsCdt && (rightIsCdt || rightIsNumeric);
    }

    public override IValuedNode Apply(params IValuedNode[] ns)
    {
        try
        {
            UCUMQuantity a = UCUMQuantityNode.ExtractQuantity(ns[0]);

            if (ns[1] is ILiteralNode rl && CdtNamespace.IsCdtQuantityDatatype(rl.DataType))
            {
                UCUMQuantity b = UCUMQuantityNode.ExtractQuantity(ns[1]);
                UCUMQuantity result = a.Multiply(b);
                return new UCUMQuantityNode(result.ToLexicalForm(), CdtNamespace.UcumUri);
            }
            else
            {
                decimal scalar = ns[1].AsDecimal();
                UCUMQuantity result = a.Multiply(scalar);
                return new UCUMQuantityNode(result.ToLexicalForm(), CdtNamespace.UcumUri);
            }
        }
        catch (UCUMException ex)
        {
            throw new RdfQueryException(ex.Message, ex);
        }
    }
}

public sealed class CdtDivisionOperator : BaseOperator
{
    public override SparqlOperatorType Operator => SparqlOperatorType.Divide;
    public override bool IsExtension => true;

    public override bool IsApplicable(params IValuedNode[] ns)
    {
        if (ns.Length != 2) return false;
        bool leftIsCdt = ns[0] is ILiteralNode l0 && CdtNamespace.IsCdtQuantityDatatype(l0.DataType);
        bool rightIsCdt = ns[1] is ILiteralNode l1 && CdtNamespace.IsCdtQuantityDatatype(l1.DataType);
        bool rightIsNumeric = ns[1] != null && ns[1].NumericType != SparqlNumericType.NaN;
        return leftIsCdt && (rightIsCdt || rightIsNumeric);
    }

    public override IValuedNode Apply(params IValuedNode[] ns)
    {
        try
        {
            UCUMQuantity a = UCUMQuantityNode.ExtractQuantity(ns[0]);

            if (ns[1] is ILiteralNode rl && CdtNamespace.IsCdtQuantityDatatype(rl.DataType))
            {
                UCUMQuantity b = UCUMQuantityNode.ExtractQuantity(ns[1]);
                UCUMQuantity result = a.Divide(b);
                return new UCUMQuantityNode(result.ToLexicalForm(), CdtNamespace.UcumUri);
            }
            else
            {
                decimal scalar = ns[1].AsDecimal();
                UCUMQuantity result = a.Divide(scalar);
                return new UCUMQuantityNode(result.ToLexicalForm(), CdtNamespace.UcumUri);
            }
        }
        catch (UCUMException ex)
        {
            throw new RdfQueryException(ex.Message, ex);
        }
    }
}