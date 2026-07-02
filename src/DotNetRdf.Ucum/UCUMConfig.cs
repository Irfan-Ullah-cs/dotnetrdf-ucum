using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
    private static volatile bool _registered;
    private static readonly object RegisterLock = new();

    private static readonly Type[] CdtOperatorTypes =
    {
        typeof(CdtAdditionOperator),
        typeof(CdtSubtractionOperator),
        typeof(CdtMultiplicationOperator),
        typeof(CdtDivisionOperator),
    };

    /// <summary>
    /// Registers the CDT arithmetic operators and the cdt:sameDimension function factory.
    /// Safe to call multiple times; subsequent calls are no-ops.
    /// </summary>
    public static void Register()
    {
        if (_registered) return;
        lock (RegisterLock)
        {
            if (_registered) return;

            SparqlOperators.AddOperator(new CdtAdditionOperator());
            SparqlOperators.AddOperator(new CdtSubtractionOperator());
            SparqlOperators.AddOperator(new CdtMultiplicationOperator());
            SparqlOperators.AddOperator(new CdtDivisionOperator());

            PrioritizeCdtOperators();

            SparqlExpressionFactory.AddCustomFactory(new CdtExpressionFactory());

            var existing = XmlSpecsHelper.SupportedTypes;
            if (!Array.Exists(existing, t => t == CdtNamespace.UcumUri.AbsoluteUri))
            {
                var extended = new string[existing.Length + 1];
                Array.Copy(existing, extended, existing.Length);
                extended[existing.Length] = CdtNamespace.UcumUri.AbsoluteUri;
                XmlSpecsHelper.SupportedTypes = extended;
            }

            // Published last, after every mutation above has completed, so any thread that
            // observes _registered == true is guaranteed to see a fully registered state.
            _registered = true;
        }
    }

    // dotNetRDF's built-in numeric operators are registered before this class runs and
    // AddOperator only appends, so they are always checked first. Since UCUMQuantityNode
    // reports a numeric type (required for CEIL/FLOOR/ROUND), the built-ins' NumericType
    // check matches CDT operands too and wins by registration order, so the Cdt*Operators
    // above never actually run. SparqlOperators has no priority API, so this reflects into
    // its private operator lists and moves the CDT operators to the front. Non-CDT operands
    // are unaffected since Cdt*Operator.IsApplicable still requires the CDT datatype.
    // Reflection target: re-verify against SparqlOperators._operators on dotNetRDF upgrades.
    private static void PrioritizeCdtOperators()
    {
        FieldInfo field = typeof(SparqlOperators).GetField(
            "_operators", BindingFlags.NonPublic | BindingFlags.Static);

        if (field?.GetValue(null) is not Dictionary<SparqlOperatorType, List<ISparqlOperator>> operators)
            return;

        foreach (SparqlOperatorType type in new[]
                 {
                     SparqlOperatorType.Add, SparqlOperatorType.Subtract,
                     SparqlOperatorType.Multiply, SparqlOperatorType.Divide,
                 })
        {
            if (!operators.TryGetValue(type, out List<ISparqlOperator> list)) continue;

            List<ISparqlOperator> cdtOps = list.Where(o => CdtOperatorTypes.Contains(o.GetType())).ToList();
            if (cdtOps.Count == 0) continue;

            list.RemoveAll(o => cdtOps.Contains(o));
            list.InsertRange(0, cdtOps);
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