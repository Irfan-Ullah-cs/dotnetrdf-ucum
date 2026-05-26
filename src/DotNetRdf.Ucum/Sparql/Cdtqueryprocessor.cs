using System.Reflection;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace DotNetRdf.Ucum;

public class CdtQueryProcessor : LeviathanQueryProcessor
{
    private static readonly FieldInfo? _orderingComparerField =
        typeof(SparqlEvaluationContext).GetField("<OrderingComparer>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance)
        ?? typeof(SparqlEvaluationContext).GetField("_orderingComparer",
            BindingFlags.NonPublic | BindingFlags.Instance);

    public CdtQueryProcessor(IInMemoryQueryableStore store)
        : base(store, UCUMConfig.ApplyQueryOptions) { }

    public CdtQueryProcessor(ISparqlDataset dataset)
        : base(dataset, UCUMConfig.CreateQueryOptions()) { }

    public override BaseMultiset ProcessOrderBy(OrderBy orderBy, SparqlEvaluationContext context)
    {
        if (context != null)
            _orderingComparerField?.SetValue(context, new CdtOrderingComparer());
        return base.ProcessOrderBy(orderBy, context);
    }
}