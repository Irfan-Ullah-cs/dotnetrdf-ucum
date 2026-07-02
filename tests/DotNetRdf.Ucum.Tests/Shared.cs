using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace DotNetRdf.Ucum.Tests;

internal static class Lit
{
    public static UCUMQuantityNode Q(string lexical) => new(lexical, CdtNamespace.UcumUri);
    public static UCUMUnitNode     U(string code)    => new(code, CdtNamespace.UcumUnitUri);
}

internal static class Sparql
{
    private static readonly SparqlQueryParser Parser = new();

    public static SparqlResultSet Query(IGraph g, string sparql)
    {
        UCUMConfig.Register();
        var store = new TripleStore();
        store.Add(g);
        var proc = new CdtQueryProcessor(store);
        return (SparqlResultSet)proc.ProcessQuery(Parser.ParseFromString(sparql));
    }

    public static SparqlResultSet Query(TripleStore store, string sparql)
    {
        UCUMConfig.Register();
        var proc = new CdtQueryProcessor(store);
        return (SparqlResultSet)proc.ProcessQuery(Parser.ParseFromString(sparql));
    }
}

internal static class G
{
    private const string EX = "https://example.org/";

    public static IGraph Make(params (string s, string p, string lexical)[] triples)
    {
        var g = new CdtGraph();
        foreach (var (s, p, l) in triples)
            g.Assert(
                g.CreateUriNode(new Uri(EX + s)),
                g.CreateUriNode(new Uri(EX + p)),
                g.CreateLiteralNode(l, CdtNamespace.UcumUri));
        return g;
    }

    public static string Uri(string local) => EX + local;
}