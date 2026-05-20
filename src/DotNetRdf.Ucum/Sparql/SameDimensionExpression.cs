using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace DotNetRdf.Ucum;

public sealed class SameDimensionExpression : ISparqlExpression
{
    private readonly ISparqlExpression _left;
    private readonly ISparqlExpression _right;

    public SameDimensionExpression(ISparqlExpression left, ISparqlExpression right)
    {
        _left = left;
        _right = right;
    }

    // Evaluates both arguments then checks dimensional compatibility.
    public TResult Accept<TResult, TContext, TBinding>(
        ISparqlExpressionProcessor<TResult, TContext, TBinding> processor,
        TContext context,
        TBinding binding)
    {
        TResult leftResult = _left.Accept(processor, context, binding);
        TResult rightResult = _right.Accept(processor, context, binding);

        if (leftResult is IValuedNode left && rightResult is IValuedNode right)
        {
            try
            {
                UCUMQuantity qa = UCUMQuantityNode.ExtractQuantity(left);
                UCUMQuantity qb = UCUMQuantityNode.ExtractQuantity(right);
                if (new BooleanNode(qa.SameDimension(qb)) is TResult result)
                    return result;
            }
            catch (RdfQueryException) { throw; }
            catch (UCUMException ex)
            {
                throw new RdfQueryException($"cdt:sameDimension evaluation failed: {ex.Message}", ex);
            }
        }

        throw new RdfQueryException("cdt:sameDimension requires two cdt:ucum typed literals");
    }

    public T Accept<T>(ISparqlExpressionVisitor<T> visitor) => visitor.VisitUnknownFunction(
        new VDS.RDF.Query.Expressions.Functions.UnknownFunction(
            CdtNamespace.SameDimensionUri, new[] { _left, _right }));

    public IEnumerable<string> Variables => _left.Variables.Concat(_right.Variables).Distinct();

    public SparqlExpressionType Type => SparqlExpressionType.Function;

    public string Functor => CdtNamespace.SameDimensionUri.AbsoluteUri;

    public IEnumerable<ISparqlExpression> Arguments => [_left, _right];

    public bool CanParallelise => _left.CanParallelise && _right.CanParallelise;

    public ISparqlExpression Transform(IExpressionTransformer transformer) =>
        new SameDimensionExpression(transformer.Transform(_left), transformer.Transform(_right));
}
