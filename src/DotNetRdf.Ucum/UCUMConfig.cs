using System;
using System.Globalization;
using System.Threading;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Operators;

namespace DotNetRdf.Ucum;

/// <summary>
/// Entry point for activating the CDT UCUM extension.
/// Call <see cref="Register"/> once at application startup, then use
/// <see cref="CdtQueryProcessor"/> for full CDT support including ORDER BY and aggregates,
/// or <see cref="CreateQueryOptions"/> when you need a plain <see cref="LeviathanQueryProcessor"/>.
/// </summary>
public static class UCUMConfig
{
    private static int _registered;

    /// <summary>
    /// Registers the CDT arithmetic operators and the cdt:sameDimension function factory.
    /// Safe to call multiple times; subsequent calls are no-ops.
    /// </summary>
    public static void Register()
    {
        if (Interlocked.CompareExchange(ref _registered, 1, 0) != 0) return;

        SparqlOperators.AddOperator(new CdtAdditionOperator());
        SparqlOperators.AddOperator(new CdtSubtractionOperator());
        SparqlOperators.AddOperator(new CdtMultiplicationOperator());
        SparqlOperators.AddOperator(new CdtDivisionOperator());

        SparqlExpressionFactory.AddCustomFactory(new CdtExpressionFactory());

        var existing = XmlSpecsHelper.SupportedTypes;
        if (!Array.Exists(existing, t => t == CdtNamespace.UcumUri.AbsoluteUri))
        {
            var extended = new string[existing.Length + 1];
            Array.Copy(existing, extended, existing.Length);
            extended[existing.Length] = CdtNamespace.UcumUri.AbsoluteUri;
            XmlSpecsHelper.SupportedTypes = extended;
        }
    }

    /// <summary>
    /// Configures a <see cref="LeviathanQueryOptions"/> instance with the CDT node comparer.
    /// Pass this as the options delegate to <see cref="LeviathanQueryProcessor"/>.
    /// </summary>
    public static void ApplyQueryOptions(LeviathanQueryOptions opts)
    {
        var inner = new SparqlNodeComparer(CultureInfo.InvariantCulture, CompareOptions.Ordinal);
        opts.NodeComparer = new CdtNodeComparer(inner);
    }

    /// <summary>
    /// Returns a <see cref="LeviathanQueryOptions"/> instance configured with the CDT
    /// node comparer. Pass this to every <see cref="LeviathanQueryProcessor"/> that should
    /// support CDT comparison operators and ORDER BY.
    /// </summary>
    public static LeviathanQueryOptions CreateQueryOptions()
    {
        var opts = new LeviathanQueryOptions();
        ApplyQueryOptions(opts);
        return opts;
    }
}