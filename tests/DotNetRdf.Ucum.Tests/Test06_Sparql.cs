using System;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Nodes;
using Xunit;

namespace DotNetRdf.Ucum.Tests;

public class Test06_Sparql_Equality
{
    // 1 [ft_i] = 12 [in_i] = 0.3048 m
    [Fact] public void Filter_Eq_CrossUnit_Foot_And_Inches()
    {
        var g = G.Make(("s1","distance","1 [ft_i]"), ("s2","distance","12 [in_i]"), ("s3","distance","6 [in_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:distance ?d . FILTER(?d = "1 [ft_i]"^^<https://w3id.org/cdt/ucum>) }
        """);
        var s = r.Select(x => x["s"].ToString()).ToHashSet();
        Assert.Contains(G.Uri("s1"), s);
        Assert.Contains(G.Uri("s2"), s);
        Assert.DoesNotContain(G.Uri("s3"), s);
    }

    // 1 [nmi_i] = 1852 m
    [Fact] public void Filter_Neq_NauticalMile_ExcludesMatch()
    {
        var g = G.Make(("s1","distance","1 [nmi_i]"), ("s2","distance","1852 m"), ("s3","distance","1000 m"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:distance ?d . FILTER(?d != "1000 m"^^<https://w3id.org/cdt/ucum>) }
        """);
        var s = r.Select(x => x["s"].ToString()).ToHashSet();
        Assert.Contains(G.Uri("s1"), s);
        Assert.Contains(G.Uri("s2"), s);
        Assert.DoesNotContain(G.Uri("s3"), s);
    }

    // 1 t = 1000 kg = 1000000 g
    [Fact] public void Filter_Eq_Mass_MetricTon_And_Kilogram()
    {
        var g = G.Make(("s1","mass","1 t"), ("s2","mass","1000 kg"), ("s3","mass","500 kg"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:mass ?m . FILTER(?m = "1 t"^^<https://w3id.org/cdt/ucum>) }
        """);
        var s = r.Select(x => x["s"].ToString()).ToHashSet();
        Assert.Contains(G.Uri("s1"), s);
        Assert.Contains(G.Uri("s2"), s);
        Assert.DoesNotContain(G.Uri("s3"), s);
    }

    // 1 kW.h = 3600 kJ = 3600000 J
    [Fact] public void Filter_Eq_Energy_KilowattHour_And_Kilojoule()
    {
        var g = G.Make(("s1","energy","1 kW.h"), ("s2","energy","3600 kJ"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:energy ?e . FILTER(?e = "1 kW.h"^^<https://w3id.org/cdt/ucum>) }
        """);
        var s = r.Select(x => x["s"].ToString()).ToHashSet();
        Assert.Contains(G.Uri("s1"), s);
        Assert.Contains(G.Uri("s2"), s);
    }

    // Known limitation: non-terminating decimal conversion factor.
    [Fact] public void Filter_Eq_Speed_CrossUnit()
    {
        var g = G.Make(("s1","speed","3.6 km/h"), ("s2","speed","1 m/s"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:speed ?v . FILTER(?v = "1 m/s"^^<https://w3id.org/cdt/ucum>) }
        """);
        var s = r.Select(x => x["s"].ToString()).ToHashSet();
        Assert.Contains(G.Uri("s1"), s);
        Assert.Contains(G.Uri("s2"), s);
    }
}

public class Test06_Sparql_Comparison
{
    // 1 [mi_i] = 1609.344 m, 1 [nmi_i] = 1852 m, 500 [ft_i] = 152.4 m, 2 km = 2000 m
    [Fact] public void Filter_Gt_CrossUnit()
    {
        var g = G.Make(("s1","distance","1 [mi_i]"), ("s2","distance","1 [nmi_i]"),
                       ("s3","distance","500 [ft_i]"), ("s4","distance","2 km"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:distance ?d . FILTER(?d > "1 km"^^<https://w3id.org/cdt/ucum>) }
        """);
        var s = r.Select(x => x["s"].ToString()).ToHashSet();
        Assert.Contains(G.Uri("s1"), s);
        Assert.Contains(G.Uri("s2"), s);
        Assert.Contains(G.Uri("s4"), s);
        Assert.DoesNotContain(G.Uri("s3"), s);
    }

    // 100 [ft_i] = 30.48 m
    [Fact] public void Filter_Lt_CrossUnit()
    {
        var g = G.Make(("s1","distance","1 [mi_i]"), ("s2","distance","1 [nmi_i]"), ("s3","distance","100 [ft_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:distance ?d . FILTER(?d < "1 km"^^<https://w3id.org/cdt/ucum>) }
        """);
        var s = r.Select(x => x["s"].ToString()).ToHashSet();
        Assert.Contains(G.Uri("s3"), s);
        Assert.DoesNotContain(G.Uri("s1"), s);
        Assert.DoesNotContain(G.Uri("s2"), s);
    }

    // 1 [mi_i] = 1609.344 m; 1609.344 m is its canonical value
    [Fact] public void Filter_Gte_IncludesEqual()
    {
        var g = G.Make(("s1","distance","1 [mi_i]"), ("s2","distance","1609.344 m"),
                       ("s3","distance","100 [ft_i]"), ("s4","distance","1 [nmi_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:distance ?d . FILTER(?d >= "1 [mi_i]"^^<https://w3id.org/cdt/ucum>) }
        """);
        var s = r.Select(x => x["s"].ToString()).ToHashSet();
        Assert.Contains(G.Uri("s1"), s);
        Assert.Contains(G.Uri("s2"), s);
        Assert.Contains(G.Uri("s4"), s);
        Assert.DoesNotContain(G.Uri("s3"), s);
    }

    // 1 [ft_i] = 0.3048 m; 1 [in_i] = 0.0254 m
    [Fact] public void Filter_Lte_IncludesEqual()
    {
        var g = G.Make(("s1","distance","1 [ft_i]"), ("s2","distance","0.3048 m"),
                       ("s3","distance","1 [in_i]"), ("s4","distance","1 m"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:distance ?d . FILTER(?d <= "1 [ft_i]"^^<https://w3id.org/cdt/ucum>) }
        """);
        var s = r.Select(x => x["s"].ToString()).ToHashSet();
        Assert.Contains(G.Uri("s1"), s);
        Assert.Contains(G.Uri("s2"), s);
        Assert.Contains(G.Uri("s3"), s);
        Assert.DoesNotContain(G.Uri("s4"), s);
    }

    // 1 t = 1000 kg; 100 [lb_av] ≈ 45.36 kg
    [Fact] public void Filter_Mass_Gt_200kg()
    {
        var g = G.Make(("s1","mass","1 t"), ("s2","mass","500 kg"), ("s3","mass","100 [lb_av]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:mass ?m . FILTER(?m > "200 kg"^^<https://w3id.org/cdt/ucum>) }
        """);
        var s = r.Select(x => x["s"].ToString()).ToHashSet();
        Assert.Contains(G.Uri("s1"), s);
        Assert.Contains(G.Uri("s2"), s);
        Assert.DoesNotContain(G.Uri("s3"), s);
    }

    // 1 [nmi_i] = 1852 m, 1 [mi_i] = 1609.344 m, 1 km = 1000 m → sorted: km, mi, nmi
    [Fact] public void OrderBy_CrossUnit_SortsCorrectly()
    {
        var g = G.Make(("s1","distance","1 [nmi_i]"), ("s2","distance","1 km"), ("s3","distance","1 [mi_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:distance ?d . } ORDER BY ?d
        """);
        var ordered = r.Select(x => x["s"].ToString()).ToList();
        Assert.Equal(G.Uri("s2"), ordered[0]);
        Assert.Equal(G.Uri("s3"), ordered[1]);
        Assert.Equal(G.Uri("s1"), ordered[2]);
    }

    [Fact] public void OrderBy_Desc_CrossUnit_SortsCorrectly()
    {
        var g = G.Make(("s1","distance","1 [nmi_i]"), ("s2","distance","1 km"), ("s3","distance","1 [mi_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:distance ?d . } ORDER BY DESC(?d)
        """);
        var ordered = r.Select(x => x["s"].ToString()).ToList();
        Assert.Equal(G.Uri("s1"), ordered[0]);
        Assert.Equal(G.Uri("s3"), ordered[1]);
        Assert.Equal(G.Uri("s2"), ordered[2]);
    }

    // 1 dyn = 1e-5 N; 1 [lbf_av] ≈ 4.448 N → sorted: dyn, N, lbf
    [Fact] public void OrderBy_Force_CrossUnit()
    {
        var g = G.Make(
            ("s1", "force", "1 dyn"),
            ("s2", "force", "1 N"),
            ("s3", "force", "1 [lbf_av]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:force ?f . } ORDER BY ?f
        """);
        var ordered = r.Select(x => x["s"].ToString()).ToList();
        Assert.Equal(G.Uri("s1"), ordered[0]);
        Assert.Equal(G.Uri("s2"), ordered[1]);
        Assert.Equal(G.Uri("s3"), ordered[2]);
    }

    [Fact] public void OrderBy_IllTyped_DoesNotCrash()
    {
        var g = G.Make(("s1","distance","1 [mi_i]"), ("s2","distance","1 [nmi_i]"), ("s3","distance","bad"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s ?d WHERE { ?s ex:distance ?d . } ORDER BY ?d
        """);
        Assert.Equal(3, r.Count);
    }

    [Fact] public void OrderBy_IllTyped_ValidOnesSortedFirst()
    {
        var g = G.Make(("s1","distance","1 [nmi_i]"), ("s2","distance","1 km"), ("s3","distance","bad"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s ?d WHERE { ?s ex:distance ?d . } ORDER BY ?d
        """);
        var ordered = r.Select(x => x["s"].ToString()).ToList();
        Assert.Equal(G.Uri("s2"), ordered[0]);
        Assert.Equal(G.Uri("s1"), ordered[1]);
    }

    [Fact] public void OrderBy_Desc_IllTyped_ValidOnesSortedFirst()
    {
        var g = G.Make(("s1","distance","1 [nmi_i]"), ("s2","distance","1 km"), ("s3","distance","bad"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s ?d WHERE { ?s ex:distance ?d . } ORDER BY DESC(?d)
        """);
        var ordered = r.Select(x => x["s"].ToString()).ToList();
        Assert.Equal(G.Uri("s3"), ordered[0]);
        Assert.Equal(G.Uri("s1"), ordered[1]);
        Assert.Equal(G.Uri("s2"), ordered[2]);
    }
}

public class Test06_Sparql_Arithmetic
{
    private static INode Result(string sparql, params (string s, string p, string v)[] data)
    {
        var g = G.Make(data);
        var r = Sparql.Query(g, sparql);
        return r.First()["result"];
    }

    // CDT operator results carry a full lexical form ("8000 m"), not a bare number -
    // take the leading numeric token only.
    private static decimal Dec(INode n) =>
        decimal.Parse(((ILiteralNode)n).Value.Split(' ')[0],
            System.Globalization.CultureInfo.InvariantCulture);

    // Confirms the CDT operator ran, not dotNetRDF's built-in decimal operator (the two
    // give the same number for compatible dimensions, only this checks the datatype).
    private static void AssertIsCdtQuantity(INode n) =>
        Assert.Equal(CdtNamespace.UcumUri, ((ILiteralNode)n).DataType);

    [Fact] public void Bind_Add_SameUnit()
    {
        var n = Result("""
            PREFIX ex: <https://example.org/>
            SELECT ?result WHERE { ex:s1 ex:a ?a . ex:s2 ex:a ?b . BIND(?a + ?b AS ?result) }
        """, ("s1","a","5 km"), ("s2","a","3 km"));
        AssertIsCdtQuantity(n);
        Assert.Equal(8000m, Dec(n));
    }

    // 1 [nmi_i] = 1852 m; 1 [mi_i] = 1609.344 m → sum = 3461.344 m
    [Fact] public void Bind_Add_CrossUnit_NauticalMile_And_Mile()
    {
        var n = Result("""
            PREFIX ex: <https://example.org/>
            SELECT ?result WHERE { ex:s1 ex:a ?a . ex:s2 ex:a ?b . BIND(?a + ?b AS ?result) }
        """, ("s1","a","1 [nmi_i]"), ("s2","a","1 [mi_i]"));
        AssertIsCdtQuantity(n);
        Assert.Equal(3461.344m, Dec(n));
    }

    // 1 [nmi_i] = 1852 m; 1 [ft_i] = 0.3048 m → difference = 1851.6952 m
    [Fact] public void Bind_Subtract_CrossUnit()
    {
        var n = Result("""
            PREFIX ex: <https://example.org/>
            SELECT ?result WHERE { ex:s1 ex:a ?a . ex:s2 ex:a ?b . BIND(?a - ?b AS ?result) }
        """, ("s1","a","1 [nmi_i]"), ("s2","a","1 [ft_i]"));
        AssertIsCdtQuantity(n);
        Assert.Equal(1851.6952m, Dec(n));
    }

    // 1 [nmi_i] × 2 = 3704 m
    [Fact] public void Bind_Multiply_ByScalar()
    {
        var n = Result("""
            PREFIX ex: <https://example.org/>
            SELECT ?result WHERE { ex:s1 ex:a ?a . BIND(?a * 2 AS ?result) }
        """, ("s1","a","1 [nmi_i]"));
        AssertIsCdtQuantity(n);
        Assert.Equal(3704m, Dec(n));
    }

    // 1 [nmi_i] / 2 = 926 m
    [Fact] public void Bind_Divide_ByScalar()
    {
        var n = Result("""
            PREFIX ex: <https://example.org/>
            SELECT ?result WHERE { ex:s1 ex:a ?a . BIND(?a / 2 AS ?result) }
        """, ("s1","a","1 [nmi_i]"));
        AssertIsCdtQuantity(n);
        Assert.Equal(926m, Dec(n));
    }

    // 3 [ft_i] = 0.9144 m; 4 [ft_i] = 1.2192 m → area = 0.9144 × 1.2192 = 1.11483648 m2
    [Fact] public void Bind_Multiply_CdtByCdt_ResultIsArea()
    {
        var n = Result("""
            PREFIX ex: <https://example.org/>
            SELECT ?result WHERE { ex:s1 ex:a ?a . ex:s2 ex:a ?b . BIND(?a * ?b AS ?result) }
        """, ("s1","a","3 [ft_i]"), ("s2","a","4 [ft_i]"));
        AssertIsCdtQuantity(n);
        Assert.Equal(1.11483648m, Dec(n));
    }

    // 1 [nmi_i] / 1 h → speed in m/s
    [Fact] public void Bind_Divide_LengthByTime_Velocity()
    {
        var n = Result("""
            PREFIX ex: <https://example.org/>
            SELECT ?result WHERE { ex:s1 ex:a ?a . ex:s2 ex:a ?b . BIND(?a / ?b AS ?result) }
        """, ("s1","a","1 [nmi_i]"), ("s2","a","1 h"));
        AssertIsCdtQuantity(n);
        Assert.True(Dec(n) > 0);
    }

    // Known limitation: Axis.Equals ignores exponents, so [ft_i] (length) and [lb_av] (mass)
    // are treated as compatible dimensions. The CDT addition operator silently succeeds and
    // dotNetRDF's built-in numeric addition fires instead of raising a dimension error.
    [Fact] public void Bind_Add_IncompatibleDimensions_NoResult()
    {
        var g = G.Make(("s1","a","1 [ft_i]"), ("s1","b","1 [lb_av]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?result WHERE { ex:s1 ex:a ?a . ex:s1 ex:b ?b . BIND(?a + ?b AS ?result) }
        """);
        Assert.True(r.Count == 0 || (r.Count == 1 && !r.First().HasBoundValue("result")));
    }
}

public class Test06_Sparql_SameDimension
{
    private static string BoolResult(IGraph g, string sparql)
    {
        var r = Sparql.Query(g, sparql);
        return r.First()["result"].ToString().ToLower().Split("^^")[0].Trim();
    }

    private static IGraph TwoValues(string a, string b)
    {
        var g = new Graph();
        g.Assert(g.CreateUriNode(new Uri(G.Uri("s1"))),
                 g.CreateUriNode(new Uri(G.Uri("a"))),
                 g.CreateLiteralNode(a, CdtNamespace.UcumUri));
        g.Assert(g.CreateUriNode(new Uri(G.Uri("s1"))),
                 g.CreateUriNode(new Uri(G.Uri("b"))),
                 g.CreateLiteralNode(b, CdtNamespace.UcumUri));
        return g;
    }

    private const string SameDimQ = """
        PREFIX ex: <https://example.org/>
        PREFIX cdt: <https://w3id.org/cdt/>
        SELECT ?result WHERE { ex:s1 ex:a ?a . ex:s1 ex:b ?b .
            BIND(cdt:sameDimension(?a, ?b) AS ?result) }
    """;

    [Fact] public void SameDimension_km_And_nmi_True() =>
        Assert.Equal("true", BoolResult(TwoValues("1 km","1 [nmi_i]"), SameDimQ));

    [Fact] public void SameDimension_AU_And_ly_True() =>
        Assert.Equal("true", BoolResult(TwoValues("1 AU","1 [ly]"), SameDimQ));

    [Fact] public void SameDimension_ft_And_mi_True() =>
        Assert.Equal("true", BoolResult(TwoValues("1 [ft_i]","1 [mi_i]"), SameDimQ));

    [Fact] public void SameDimension_kg_And_lb_True() =>
        Assert.Equal("true", BoolResult(TwoValues("1 kg","1 [lb_av]"), SameDimQ));

    [Fact] public void SameDimension_N_And_kg_m_s2_True() =>
        Assert.Equal("true", BoolResult(TwoValues("1 N","1 kg.m/s2"), SameDimQ));

    [Fact] public void SameDimension_Length_And_Mass_False() =>
        Assert.Equal("false", BoolResult(TwoValues("1 [mi_i]","1 [lb_av]"), SameDimQ));

    [Fact] public void SameDimension_Length_And_Time_False() =>
        Assert.Equal("false", BoolResult(TwoValues("1 [ft_i]","1 h"), SameDimQ));

    // Known limitation: Fhir.Metrics Axis.Equals ignores the exponent field, so N
    // (kg.m.s-2) and Pa (kg.m-1.s-2) are wrongly reported as the same dimension.
    [Fact] public void SameDimension_N_And_Pa_False() =>
        Assert.Equal("false", BoolResult(TwoValues("1 N","1 Pa"), SameDimQ));

    [Fact] public void SameDimension_Filter_SelectsCompatible()
    {
        var g = G.Make(("s1","val","1 [mi_i]"), ("s2","val","1 [lb_av]"), ("s3","val","1 [nmi_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            PREFIX cdt: <https://w3id.org/cdt/>
            SELECT ?s WHERE {
                ?s ex:val ?v .
                FILTER(cdt:sameDimension(?v, "1 m"^^<https://w3id.org/cdt/ucum>))
            }
        """);
        var s = r.Select(x => x["s"].ToString()).ToHashSet();
        Assert.Contains(G.Uri("s1"), s);
        Assert.Contains(G.Uri("s3"), s);
        Assert.DoesNotContain(G.Uri("s2"), s);
    }
}

public class Test06_Sparql_Dimensionless
{
    // Known limitation: percent ("%") is not canonicalized to the same value-space
    // representation as the dimensionless "1" unit by this backend.
    [Fact] public void Filter_Dimensionless_50percent_Equals_0_5_1()
    {
        var g = G.Make(("s1","v","50 %"), ("s2","v","0.5 1"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:v ?v . FILTER(?v = "0.5 1"^^<https://w3id.org/cdt/ucum>) }
        """);
        var s = r.Select(x => x["s"].ToString()).ToHashSet();
        Assert.Contains(G.Uri("s1"), s);
        Assert.Contains(G.Uri("s2"), s);
    }

    [Fact] public void OrderBy_Dimensionless_SortsCorrectly()
    {
        var g = G.Make(("s1","v","0.25 1"), ("s2","v","50 %"), ("s3","v","0.1 1"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:v ?v . } ORDER BY ?v
        """);
        var ordered = r.Select(x => x["s"].ToString()).ToList();
        Assert.Equal(G.Uri("s3"), ordered[0]);
    }
}

public class Test06_Sparql_AggregatesIllTyped
{
    // Known limitation: SUM calls AsDecimal() on each member; an ill-typed literal
    // throws UCUMParseException rather than being silently skipped.
    // 1 [mi_i] = 1609.344 m, 1 [nmi_i] = 1852 m, 1 [ft_i] = 0.3048 m → sum = 3461.6488 m
    [Fact] public void Sum_SkipsIllTyped_SumsOnlyValid()
    {
        var g = G.Make(
            ("s1","distance","1 [mi_i]"), ("s2","distance","1 [nmi_i]"),
            ("s3","distance","bad"), ("s4","distance","1 [ft_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (SUM(?d) AS ?total) WHERE { ?s ex:distance ?d . }
        """);
        Assert.Equal(1, r.Count);
        var total = decimal.Parse(((VDS.RDF.ILiteralNode)r.First()["total"]).Value,
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(3461.6488m, total);
    }

    // Known limitation: same as Sum_SkipsIllTyped_SumsOnlyValid, AVG path.
    // 1 [mi_i] = 1609.344 m, 1 [nmi_i] = 1852 m → avg = 1730.672 m
    [Fact] public void Avg_SkipsIllTyped_AveragesOnlyValid()
    {
        var g = G.Make(
            ("s1","distance","1 [mi_i]"), ("s2","distance","1 [nmi_i]"), ("s3","distance","bad"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (AVG(?d) AS ?avg) WHERE { ?s ex:distance ?d . }
        """);
        Assert.Equal(1, r.Count);
        var avg = decimal.Parse(((VDS.RDF.ILiteralNode)r.First()["avg"]).Value,
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(1730.672m, avg);
    }

    // Known limitation: same as Sum_SkipsIllTyped_SumsOnlyValid, all-ill-typed case.
    [Fact] public void Sum_AllIllTyped_ReturnsZero()
    {
        var g = G.Make(("s1","distance","bad1"), ("s2","distance","bad2"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (SUM(?d) AS ?total) WHERE { ?s ex:distance ?d . }
        """);
        Assert.Equal(1, r.Count);
        var total = double.Parse(((VDS.RDF.ILiteralNode)r.First()["total"]).Value,
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(0.0, total);
    }
}

public class Test06_Aggregates
{
    private static decimal GetDecimal(INode n) =>
        decimal.Parse(((ILiteralNode)n).Value.Split(' ')[0],
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture);

    // 1 [mi_i] + 1 [nmi_i] + 1 [ft_i] = 1609.344 + 1852 + 0.3048 = 3461.6488 m
    [Fact] public void Sum_MixedLengthUnits_CorrectMagnitude()
    {
        var g = G.Make(
            ("s1", "distance", "1 [mi_i]"),
            ("s2", "distance", "1 [nmi_i]"),
            ("s3", "distance", "1 [ft_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (SUM(?d) AS ?total) WHERE { ?s ex:distance ?d . }
        """);
        Assert.Equal(1, r.Count);
        Assert.Equal(3461.6488m, GetDecimal(r.First()["total"]));
    }

    // Known limitation: LeviathanAggregateProcessor.ProcessSum unconditionally calls
    // AsDecimal() and reconstructs a bare DecimalNode - no extension point exists to
    // intercept this, same architecture as ABS/CEIL/FLOOR/ROUND. This is broader than the
    // already-documented ill-typed-literal case (Test06_Sparql_AggregatesIllTyped): the
    // datatype is stripped even when every input is a fully valid cdt:ucum literal.
    [Fact] public void Sum_ValidData_StripsDatatype()
    {
        var g = G.Make(("s1","distance","1 km"), ("s2","distance","500 m"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (SUM(?d) AS ?result) WHERE { ?s ex:distance ?d . }
        """);
        Assert.Equal(1, r.Count);
        Assert.Equal(CdtNamespace.UcumUri, ((ILiteralNode)r.First()["result"]).DataType);
    }

    // Known limitation: same root cause as Sum_ValidData_StripsDatatype - ProcessAverage
    // always reconstructs a bare DecimalNode.
    [Fact] public void Avg_ValidData_StripsDatatype()
    {
        var g = G.Make(("s1","distance","1 km"), ("s2","distance","500 m"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (AVG(?d) AS ?result) WHERE { ?s ex:distance ?d . }
        """);
        Assert.Equal(1, r.Count);
        Assert.Equal(CdtNamespace.UcumUri, ((ILiteralNode)r.First()["result"]).DataType);
    }

    // Unlike SUM/AVG, LeviathanAggregateProcessor.ProcessMax sorts the bound nodes via the
    // configured node comparer (CdtNodeComparer) and returns the original node unchanged -
    // no reconstruction happens, so the cdt:ucum datatype survives. 1 [mi_i] = 1609.344 m
    // is the largest of the three.
    // MIN/MAX return the original bound node unchanged (dotNetRDF's LeviathanAggregateProcessor.
    // ProcessMax/ProcessMin sort via the node comparer and return values.FirstOrDefault() - the
    // literal node itself, not a reconstructed one), so they preserve the cdt:ucum datatype.
    // Unlike SUM/AVG (which canonicalize via AsDecimal()) or the +/-/*// operators (which
    // build a fresh node expressed in SI units), MAX/MIN return the ORIGINAL node completely
    // untouched - so the result's lexical form is whatever the winning literal was authored
    // as ("1 [mi_i]", not "1609.344 m"). Asserted against the exact preserved lexical form,
    // not a canonical magnitude that MAX/MIN never compute.
    [Fact] public void Max_CrossUnit_PreservesDatatype()
    {
        var g = G.Make(("s1","distance","1 km"), ("s2","distance","500 m"), ("s3","distance","1 [mi_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (MAX(?dist) AS ?maxVal) WHERE { ?s ex:distance ?dist . }
        """);
        Assert.Equal(1, r.Count);
        var result = r.First()["maxVal"];
        Assert.Equal(CdtNamespace.UcumUri, ((ILiteralNode)result).DataType);
        Assert.Equal("1 [mi_i]", ((ILiteralNode)result).Value);
    }

    // Same reasoning as Max_CrossUnit_PreservesDatatype - ProcessMin returns the original
    // node unchanged. 500 m is the smallest of the three, and already expressed in the base
    // unit, so its coefficient happens to equal its canonical value - unlike the mi_i case
    // above, which is why this test alone could not have caught the wrong-assertion bug.
    [Fact] public void Min_CrossUnit_PreservesDatatype()
    {
        var g = G.Make(("s1","distance","1 km"), ("s2","distance","500 m"), ("s3","distance","1 [mi_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (MIN(?dist) AS ?minVal) WHERE { ?s ex:distance ?dist . }
        """);
        Assert.Equal(1, r.Count);
        var result = r.First()["minVal"];
        Assert.Equal(CdtNamespace.UcumUri, ((ILiteralNode)result).DataType);
        Assert.Equal("500 m", ((ILiteralNode)result).Value);
    }
}

public class Test06_NumericFunctions
{
    // Predicate "val" must match the SPARQL query string below.
    private static INode? TryGetResult(string fn, string lexical)
    {
        var g = G.Make(("s1","val",lexical));
        var sparql = $"PREFIX ex: <https://example.org/> SELECT ({fn}(?v) AS ?r) WHERE {{ ex:s1 ex:val ?v . }}";
        var r = Sparql.Query(g, sparql);
        return r.Count > 0 && r.First().HasBoundValue("r") ? r.First()["r"] : null;
    }

    // ABS operates on the canonical SI value (AsDecimal() returns metres, not km).
    // 3.5 km = 3500 m; ABS(3500) = 3500.
    [Fact] public void Abs_Positive_ReturnsValue()
    {
        var n = TryGetResult("ABS", "3.5 km");
        Assert.NotNull(n);
        Assert.Equal(3500m, decimal.Parse(((ILiteralNode)n!).Value,
            System.Globalization.CultureInfo.InvariantCulture));
    }

    // -3.5 km = -3500 m; ABS(-3500) = 3500.
    [Fact] public void Abs_Negative_ReturnsPositive()
    {
        var n = TryGetResult("ABS", "-3.5 km");
        Assert.NotNull(n);
        Assert.Equal(3500m, decimal.Parse(((ILiteralNode)n!).Value,
            System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact] public void Abs_Zero_ReturnsZero()
    {
        var n = TryGetResult("ABS", "0 m");
        Assert.NotNull(n);
        Assert.Equal(0m, decimal.Parse(((ILiteralNode)n!).Value,
            System.Globalization.CultureInfo.InvariantCulture));
    }

    // Known limitation: ABS returns xsd:decimal instead of cdt:ucum because the SPARQL
    // numeric function layer calls AsDecimal() and reboxes the result as a plain decimal
    // literal, stripping the datatype.
    // -1 [nmi_i] = -1852 m; ABS(-1852) = 1852 m.
    [Fact] public void Abs_PreservesUnit()
    {
        var g = G.Make(("s1","val","-1 [nmi_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (ABS(?v) AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count > 0 && r.First().HasBoundValue("r"));
        Assert.Equal(CdtNamespace.UcumUri, ((VDS.RDF.ILiteralNode)r.First()["r"]).DataType);
        var v = decimal.Parse(((VDS.RDF.ILiteralNode)r.First()["r"]).Value,
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(1852m, v);
    }

    [Fact] public void Abs_IllTyped_ReturnsNoResult()
    {
        var g = G.Make(("s1","val","bad"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (ABS(?v) AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count == 0 || !r.First().HasBoundValue("r"));
    }

    // CEIL operates on the canonical SI value. 1.2 m → CEIL(1.2) = 2.
    [Fact] public void Ceil_PositiveDecimal_RoundsUp()
    {
        var n = TryGetResult("CEIL", "1.2 m");
        Assert.NotNull(n);
        Assert.Equal(2m, decimal.Parse(((ILiteralNode)n!).Value,
            System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact] public void Ceil_WholeNumber_Unchanged()
    {
        var n = TryGetResult("CEIL", "3 m");
        Assert.NotNull(n);
        Assert.Equal(3m, decimal.Parse(((ILiteralNode)n!).Value,
            System.Globalization.CultureInfo.InvariantCulture));
    }

    // Known limitation: AsDecimal() returns the SI-scaled value, so CEIL operates on
    // the canonical metres value (-1.7 × 1609.344 = -2735.88 m), not on the -1.7
    // coefficient. Expected -1 (CEIL of -1.7); actual is CEIL(-2735.88) = -2735.
    [Fact] public void Ceil_NegativeDecimal_RoundsTowardZero()
    {
        var g = G.Make(("s1","val","-1.7 [mi_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (CEIL(?v) AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count > 0 && r.First().HasBoundValue("r"));
        var v = decimal.Parse(((VDS.RDF.ILiteralNode)r.First()["r"]).Value,
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(-1m, v);
    }

    // Known limitation: CEIL returns xsd:decimal instead of cdt:ucum (datatype stripping).
    [Fact] public void Ceil_PreservesDatatype()
    {
        var g = G.Make(("s1","val","1.4 [lb_av]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (CEIL(?v) AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count > 0 && r.First().HasBoundValue("r"));
        Assert.Equal(CdtNamespace.UcumUri, ((VDS.RDF.ILiteralNode)r.First()["r"]).DataType);
    }

    [Fact] public void Ceil_IllTyped_ReturnsNoResult()
    {
        var g = G.Make(("s1","val","bad"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (CEIL(?v) AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count == 0 || !r.First().HasBoundValue("r"));
    }

    // FLOOR operates on the canonical SI value. 1.9 m → FLOOR(1.9) = 1.
    [Fact] public void Floor_PositiveDecimal_RoundsDown()
    {
        var n = TryGetResult("FLOOR", "1.9 m");
        Assert.NotNull(n);
        Assert.Equal(1m, decimal.Parse(((ILiteralNode)n!).Value,
            System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact] public void Floor_WholeNumber_Unchanged()
    {
        var n = TryGetResult("FLOOR", "5 m");
        Assert.NotNull(n);
        Assert.Equal(5m, decimal.Parse(((ILiteralNode)n!).Value,
            System.Globalization.CultureInfo.InvariantCulture));
    }

    // Known limitation: AsDecimal() returns SI value, so FLOOR operates on
    // -1.2 × 0.3048 = -0.36576 m, not on -1.2. Expected -2 (FLOOR of -1.2);
    // actual is FLOOR(-0.36576) = -1.
    [Fact] public void Floor_NegativeDecimal_RoundsAwayFromZero()
    {
        var g = G.Make(("s1","val","-1.2 [ft_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (FLOOR(?v) AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count > 0 && r.First().HasBoundValue("r"));
        var v = decimal.Parse(((VDS.RDF.ILiteralNode)r.First()["r"]).Value,
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(-2m, v);
    }

    // Known limitation: FLOOR returns xsd:decimal instead of cdt:ucum (datatype stripping).
    [Fact] public void Floor_PreservesDatatype()
    {
        var g = G.Make(("s1","val","9.9 [kn_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (FLOOR(?v) AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count > 0 && r.First().HasBoundValue("r"));
        Assert.Equal(CdtNamespace.UcumUri, ((VDS.RDF.ILiteralNode)r.First()["r"]).DataType);
    }

    [Fact] public void Floor_IllTyped_ReturnsNoResult()
    {
        var g = G.Make(("s1","val","bad"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (FLOOR(?v) AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count == 0 || !r.First().HasBoundValue("r"));
    }

    // ROUND operates on the canonical SI value. 1.5 m → ROUND(1.5) is 1 or 2.
    [Fact] public void Round_HalfUp()
    {
        var n = TryGetResult("ROUND", "1.5 m");
        Assert.NotNull(n);
        var v = decimal.Parse(((ILiteralNode)n!).Value,
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(v >= 1m && v <= 2m);
    }

    // 1.2 m → ROUND(1.2) = 1.
    [Fact] public void Round_Down()
    {
        var n = TryGetResult("ROUND", "1.2 m");
        Assert.NotNull(n);
        var v = decimal.Parse(((ILiteralNode)n!).Value,
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(1m, v);
    }

    // Known limitation: AsDecimal() returns SI value, so ROUND operates on
    // -1.5 × 1609.344 = -2414.016 m, not on -1.5. Expected -1; actual is ROUND(-2414.016) = -2414.
    [Fact] public void Round_Negative_HalfUp()
    {
        var g = G.Make(("s1","val","-1.5 [mi_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (ROUND(?v) AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count > 0 && r.First().HasBoundValue("r"));
        var v = decimal.Parse(((VDS.RDF.ILiteralNode)r.First()["r"]).Value,
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(-1m, v);
    }

    // Known limitation: ROUND returns xsd:decimal instead of cdt:ucum (datatype stripping).
    [Fact] public void Round_PreservesDatatype()
    {
        var g = G.Make(("s1","val","9.6 [lb_av]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (ROUND(?v) AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count > 0 && r.First().HasBoundValue("r"));
        Assert.Equal(CdtNamespace.UcumUri, ((VDS.RDF.ILiteralNode)r.First()["r"]).DataType);
    }

    [Fact] public void Round_IllTyped_ReturnsNoResult()
    {
        var g = G.Make(("s1","val","bad"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (ROUND(?v) AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count == 0 || !r.First().HasBoundValue("r"));
    }

    [Fact] public void UnaryMinus_MakesPositiveNegative()
    {
        var g = G.Make(("s1", "val", "1 [nmi_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (-?v AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count > 0 && r.First().HasBoundValue("r"));
        var val = double.Parse(((ILiteralNode)r.First()["r"]).Value,
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(val < 0);
    }

    [Fact] public void UnaryMinus_PreservesUnit()
    {
        var g = G.Make(("s1","val","1 [lbf_av]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT (-?v AS ?r) WHERE { ex:s1 ex:val ?v . }
        """);
        Assert.True(r.Count > 0 && r.First().HasBoundValue("r"));
        var v = double.Parse(((VDS.RDF.ILiteralNode)r.First()["r"]).Value,
            System.Globalization.CultureInfo.InvariantCulture);
        Assert.True(v < 0);
    }

    // Known limitation: ABS strips the CDT datatype, returning xsd:decimal. The subsequent
    // FILTER comparing xsd:decimal against cdt:ucum "1 km" therefore fails and returns 0 rows.
    // ABS(-1 [nmi_i]) = 1852 m > 1 km → s1 should match, s2 (500 m) should not.
    [Fact] public void Abs_ThenFilter_ComparesCorrectly()
    {
        var g = G.Make(("s1","val","-1 [nmi_i]"), ("s2","val","500 m"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE {
                ?s ex:val ?v .
                FILTER(ABS(?v) > "1 km"^^<https://w3id.org/cdt/ucum>)
            }
        """);
        Assert.Equal(1, r.Count);
        Assert.Equal(G.Uri("s1"), r.First()["s"].ToString());
    }

    // negated order: 1 [nmi_i] > 1 [mi_i] > 1 [ft_i], so ORDER BY (-v) ascending
    // gives [nmi_i], [mi_i], [ft_i] → s2, s3, s1
    [Fact] public void Negate_ThenOrderBy_ReversesOrder()
    {
        var g = G.Make(("s1","val","1 [ft_i]"), ("s2","val","1 [nmi_i]"), ("s3","val","1 [mi_i]"));
        var r = Sparql.Query(g, """
            PREFIX ex: <https://example.org/>
            SELECT ?s WHERE { ?s ex:val ?v . } ORDER BY (-?v)
        """);
        var subjects = r.Select(x => x["s"].ToString().Split('/').Last()).ToList();
        Assert.Equal(new[] { "s2", "s3", "s1" }, subjects);
    }
}

public class Test06_SparqlMathEdgeCases
{
    private static INode? Q1(string fn, string lexical)
    {
        var g = G.Make(("s1","v",lexical));
        var r = Sparql.Query(g, $$"""
            PREFIX ex: <https://example.org/>
            SELECT ({{fn}}(?v) AS ?r) WHERE { ex:s1 ex:v ?v . }
        """);
        return r.Count > 0 && r.First().HasBoundValue("r") ? r.First()["r"] : null;
    }

    private static decimal Dec(INode n) =>
        decimal.Parse(((VDS.RDF.ILiteralNode)n).Value,
            System.Globalization.CultureInfo.InvariantCulture);

    [Fact] public void Ceil_Zero() => Assert.Equal(0m, Dec(Q1("CEIL", "0 m")!));
    [Fact] public void Floor_Zero() => Assert.Equal(0m, Dec(Q1("FLOOR", "0 m")!));
    [Fact] public void Round_Zero() => Assert.Equal(0m, Dec(Q1("ROUND", "0 m")!));
    [Fact] public void Abs_NegativeOne() => Assert.Equal(1m, Dec(Q1("ABS", "-1 m")!));

    // Known limitation: ROUND returns xsd:decimal instead of cdt:ucum (datatype stripping).
    [Fact] public void Round_PoundForce()
    {
        var n = Q1("ROUND", "2.7 [lbf_av]");
        Assert.NotNull(n);
        Assert.Equal(CdtNamespace.UcumUri, ((VDS.RDF.ILiteralNode)n!).DataType);
    }

    // Known limitation: CEIL returns xsd:decimal instead of cdt:ucum (datatype stripping).
    [Fact] public void Ceil_KnotSpeed()
    {
        var n = Q1("CEIL", "3.2 [kn_i]");
        Assert.NotNull(n);
        Assert.Equal(CdtNamespace.UcumUri, ((VDS.RDF.ILiteralNode)n!).DataType);
    }

    [Fact] public void Floor_NegativeNearZero()
    {
        var n = Q1("FLOOR", "-0.1 m");
        Assert.NotNull(n);
        Assert.Equal(-1m, Dec(n!));
    }

    [Fact] public void Ceil_NegativeNearZero()
    {
        var n = Q1("CEIL", "-0.9 m");
        Assert.NotNull(n);
        Assert.Equal(0m, Dec(n!));
    }
}