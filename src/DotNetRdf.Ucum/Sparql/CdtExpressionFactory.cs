using System;
using System.Collections.Generic;
using VDS.RDF.Query.Expressions;

namespace DotNetRdf.Ucum;

public sealed class CdtExpressionFactory : ISparqlCustomExpressionFactory
{
    public IEnumerable<Uri> AvailableExtensionFunctions { get; } =
        [CdtNamespace.SameDimensionUri];

    public IEnumerable<Uri> AvailableExtensionAggregates { get; } =
        Array.Empty<Uri>();

    public bool TryCreateExpression(
        Uri u,
        List<ISparqlExpression> args,
        Dictionary<string, ISparqlExpression> scalarArguments,
        out ISparqlExpression expr)
    {
        if (u.AbsoluteUri == CdtNamespace.SameDimensionUri.AbsoluteUri && args.Count == 2)
        {
            expr = new SameDimensionExpression(args[0], args[1]);
            return true;
        }

        expr = null!;
        return false;
    }
}
