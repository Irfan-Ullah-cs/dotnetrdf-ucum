using System;
using System.Globalization;
using VDS.RDF.Query;
using Xunit;

namespace DotNetRdf.Ucum.Tests;

public class Test04_Comparison
{
    private static CdtNodeComparer Cmp() =>
        new(new SparqlNodeComparer(CultureInfo.InvariantCulture, CompareOptions.Ordinal));

    [Fact] public void CompareTo_Equal_Zero() =>
        Assert.Equal(0, new UCUMQuantity(1000m, "m").CompareTo(new UCUMQuantity(1000m, "m")));

    [Fact] public void CompareTo_Greater_Positive() =>
        Assert.True(new UCUMQuantity(2000m, "m").CompareTo(new UCUMQuantity(1000m, "m")) > 0);

    [Fact] public void CompareTo_Less_Negative() =>
        Assert.True(new UCUMQuantity(500m, "m").CompareTo(new UCUMQuantity(1000m, "m")) < 0);

    [Fact] public void CompareTo_IncompatibleDimensions_Throws() =>
        Assert.Throws<UCUMDimensionException>(
            () => new UCUMQuantity(1m, "m").CompareTo(new UCUMQuantity(1m, "s")));

    [Fact] public void Comparer_1km_Eq_1000m()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("1 km"), Lit.Q("1000 m"), out int r));
        Assert.Equal(0, r);
    }

    [Fact] public void Comparer_2km_Gt_500m()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("2 km"), Lit.Q("500 m"), out int r));
        Assert.True(r > 0);
    }

    [Fact] public void Comparer_100m_Lt_1km()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("100 m"), Lit.Q("1 km"), out int r));
        Assert.True(r < 0);
    }

    [Fact] public void Comparer_500g_Lt_1kg()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("500 g"), Lit.Q("1 kg"), out int r));
        Assert.True(r < 0);
    }

    [Fact] public void Comparer_59min_Lt_1h()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("59 min"), Lit.Q("1 h"), out int r));
        Assert.True(r < 0);
    }

    [Fact] public void Comparer_1kg_Gt_999g()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("1 kg"), Lit.Q("999 g"), out int r));
        Assert.True(r > 0);
    }

    [Fact] public void Comparer_1kJ_Gt_1J()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("1 kJ"), Lit.Q("1 J"), out int r));
        Assert.True(r > 0);
    }

    [Fact] public void Comparer_2N_Gt_1N()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("2 N"), Lit.Q("1 N"), out int r));
        Assert.True(r > 0);
    }

    [Fact] public void Comparer_100Hz_Gt_10Hz()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("100 Hz"), Lit.Q("10 Hz"), out int r));
        Assert.True(r > 0);
    }

    [Fact] public void Comparer_100s_minus1_Gt_10Hz()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("100 s-1"), Lit.Q("10 Hz"), out int r));
        Assert.True(r > 0);
    }

    [Fact] public void Comparer_IncompatibleDimensions_ReturnsFalse() =>
        Assert.False(Cmp().TryCompare(Lit.Q("1 m"), Lit.Q("1 s"), out _));

    [Fact] public void Compound_9_81_Gt_9_0_m_s2()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("9.81 m/s2"), Lit.Q("9.0 m/s2"), out int r));
        Assert.True(r > 0);
    }

    [Fact] public void Compound_500g_m_s2_Lt_1kg_m_s2()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("500 g.m/s2"), Lit.Q("1 kg.m/s2"), out int r));
        Assert.True(r < 0);
    }

    [Fact] public void Compound_10kg_m_s2_Gt_5N()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("10 kg.m/s2"), Lit.Q("5 N"), out int r));
        Assert.True(r > 0);
    }

    [Fact] public void Kelvin_300K_Gt_273_15K()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("300 K"), Lit.Q("273.15 K"), out int r));
        Assert.True(r > 0);
    }

    [Fact] public void Kelvin_100K_Lt_200K()
    {
        Assert.True(Cmp().TryCompare(Lit.Q("100 K"), Lit.Q("200 K"), out int r));
        Assert.True(r < 0);
    }

    [Fact] public void Operator_Lt_500m_Lt_1km() =>
        Assert.True(UCUMQuantity.Parse("500 m") < UCUMQuantity.Parse("1 km"));

    [Fact] public void Operator_Lt_500g_Lt_1kg() =>
        Assert.True(UCUMQuantity.Parse("500 g") < UCUMQuantity.Parse("1 kg"));

    [Fact] public void Operator_Lt_59min_Lt_1h() =>
        Assert.True(UCUMQuantity.Parse("59 min") < UCUMQuantity.Parse("1 h"));

    [Fact] public void Operator_Lt_1eV_Lt_1J() =>
        Assert.True(UCUMQuantity.Parse("1 eV") < UCUMQuantity.Parse("1 J"));

    [Fact] public void Operator_Lt_False_When_Greater() =>
        Assert.False(UCUMQuantity.Parse("1 km") < UCUMQuantity.Parse("500 m"));

    [Fact] public void Operator_Lt_False_When_Equal() =>
        Assert.False(UCUMQuantity.Parse("1 km") < UCUMQuantity.Parse("1000 m"));

    [Fact] public void Operator_Lt_IncompatibleDimensions_Throws() =>
        Assert.Throws<UCUMDimensionException>(
            () => UCUMQuantity.Parse("1 m") < UCUMQuantity.Parse("1 kg"));

    [Fact] public void Operator_Gt_1kg_Gt_999g() =>
        Assert.True(UCUMQuantity.Parse("1 kg") > UCUMQuantity.Parse("999 g"));

    [Fact] public void Operator_Gt_10ms_Gt_30kmh() =>
        Assert.True(UCUMQuantity.Parse("10 m/s") > UCUMQuantity.Parse("30 km/h"));

    [Fact] public void Operator_Gt_9_81_Gt_9_0_m_s2() =>
        Assert.True(UCUMQuantity.Parse("9.81 m/s2") > UCUMQuantity.Parse("9.0 m/s2"));

    [Fact] public void Operator_Gt_False_When_Equal() =>
        Assert.False(UCUMQuantity.Parse("1000 m") > UCUMQuantity.Parse("1 km"));

    [Fact] public void Operator_Gt_IncompatibleDimensions_Throws() =>
        Assert.Throws<UCUMDimensionException>(
            () => UCUMQuantity.Parse("1 m") > UCUMQuantity.Parse("1 s"));

    [Fact] public void Operator_Le_Equal_CrossUnit() =>
        Assert.True(UCUMQuantity.Parse("1000 m") <= UCUMQuantity.Parse("1 km"));

    [Fact] public void Operator_Le_False_When_Greater() =>
        Assert.False(UCUMQuantity.Parse("2 km") <= UCUMQuantity.Parse("1000 m"));

    [Fact] public void Operator_Le_IncompatibleDimensions_Throws() =>
        Assert.Throws<UCUMDimensionException>(
            () => UCUMQuantity.Parse("1 m") <= UCUMQuantity.Parse("1 kg"));

    [Fact] public void Operator_Ge_Equal_CrossUnit() =>
        Assert.True(UCUMQuantity.Parse("1 km") >= UCUMQuantity.Parse("1000 m"));

    [Fact] public void Operator_Ge_False_When_Less() =>
        Assert.False(UCUMQuantity.Parse("500 m") >= UCUMQuantity.Parse("1 km"));

    [Fact] public void Operator_Ge_IncompatibleDimensions_Throws() =>
        Assert.Throws<UCUMDimensionException>(
            () => UCUMQuantity.Parse("1 m") >= UCUMQuantity.Parse("1 s"));

    [Fact] public void Dimensionless_0_5_Lt_1() =>
        Assert.True(UCUMQuantity.Parse("0.5 1") < UCUMQuantity.Parse("1 1"));


    // Fhir.Metrics overflow — decimal max is ~7.9e28.
    [Fact] public void LargeValue_1E1000_Throws() =>
        Assert.Throws<UCUMParseException>(() => UCUMQuantity.Parse("1e1000 m"));


    // Fhir.Metrics uses double internally - 1e-1023 underflows to 0.0 before decimal is reached.
    [Fact] public void SmallValue_1E_Minus1023_Throws() =>
        Assert.Throws<UCUMParseException>(() => UCUMQuantity.Parse("1e-1023 m"));

    [Fact(Skip = "Celsius offset conversion not supported by Fhir.Metrics (special unit, additive conversion formula).")]
    public void Kelvin_300K_Gt_0Celsius()
    {
        var a = UCUMQuantity.Parse("300 K");
        var b = UCUMQuantity.Parse("0 Cel");
        Assert.True(a > b);
    }
}