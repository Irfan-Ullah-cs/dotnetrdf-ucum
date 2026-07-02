using System;
using System.Collections.Generic;
using Xunit;

namespace DotNetRdf.Ucum.Tests;

public class Test03_Equality
{
    [Fact] public void Eq_1km_And_1000m() =>
        Assert.True(UCUMOperations.Equals(Lit.Q("1 km"), Lit.Q("1000 m")));

    [Fact] public void Eq_70kg_And_70000g() =>
        Assert.True(UCUMOperations.Equals(Lit.Q("70 kg"), Lit.Q("70000 g")));

    [Fact] public void Eq_1h_And_3600s() =>
        Assert.True(UCUMOperations.Equals(Lit.Q("1 h"), Lit.Q("3600 s")));

    [Fact] public void Eq_1kPa_And_1000Pa() =>
        Assert.True(UCUMOperations.Equals(Lit.Q("1 kPa"), Lit.Q("1000 Pa")));

    [Fact] public void Neq_1km_And_500m() =>
        Assert.False(UCUMOperations.Equals(Lit.Q("1 km"), Lit.Q("500 m")));

    [Fact] public void Neq_IncompatibleDimensions_DoesNotThrow()
    {
        var cmp = UCUMConfig.CreateQueryOptions().NodeComparer;
        bool ok = cmp.TryCompare(Lit.Q("1 m"), Lit.Q("1 kg"), out _);
        Assert.False(ok);
    }

    [Fact] public void SameDimension_km_And_m_True() =>
        Assert.True(UCUMOperations.SameDimension(Lit.Q("1 km"), Lit.Q("500 m")));

    [Fact] public void SameDimension_kg_And_g_True() =>
        Assert.True(UCUMOperations.SameDimension(Lit.Q("1 kg"), Lit.Q("500 g")));

    [Fact] public void SameDimension_h_And_s_True() =>
        Assert.True(UCUMOperations.SameDimension(Lit.Q("1 h"), Lit.Q("3600 s")));

    [Fact] public void SameDimension_N_And_kg_m_s2_True() =>
        Assert.True(UCUMOperations.SameDimension(Lit.Q("1 N"), Lit.Q("1 kg.m/s2")));

    [Fact] public void SameDimension_m_And_kg_False() =>
        Assert.False(UCUMOperations.SameDimension(Lit.Q("1 m"), Lit.Q("1 kg")));

    [Fact] public void SameDimension_m_And_s_False() =>
        Assert.False(UCUMOperations.SameDimension(Lit.Q("1 m"), Lit.Q("1 s")));

    // Known limitation: Fhir.Metrics Axis.Equals ignores the exponent field entirely,
    // so units sharing the same base axes but different exponents (e.g. N=kg.m.s-2 vs
    // Pa=kg.m-1.s-2) are incorrectly reported as the same dimension.
    [Fact]
    public void SameDimension_N_And_Pa_False() =>
        Assert.False(UCUMOperations.SameDimension(Lit.Q("1 N"), Lit.Q("1 Pa")));

    [Fact]
    public void SameDimension_N_And_J_False() =>
        Assert.False(UCUMOperations.SameDimension(Lit.Q("1 N"), Lit.Q("1 J")));

    [Fact]
    public void SameDimension_Pa_And_J_False() =>
        Assert.False(UCUMOperations.SameDimension(Lit.Q("1 Pa"), Lit.Q("1 J")));

    [Fact]
    public void SameDimension_m_s_minus1_And_m_s_minus2_False() =>
        Assert.False(UCUMOperations.SameDimension(Lit.Q("1 m/s"), Lit.Q("1 m/s2")));

    [Fact] public void Hash_SameValueUnit_Equal()
    {
        var a = new UCUMQuantity(1000m, "m");
        var b = new UCUMQuantity(1000m, "m");
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact] public void Hash_DifferentValues_LikelyDifferent()
    {
        var a = new UCUMQuantity(1000m, "m");
        var b = new UCUMQuantity(2000m, "m");
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [Fact] public void Hash_UsableAsDictionaryKey()
    {
        var a = new UCUMQuantity(1000m, "m");
        var b = new UCUMQuantity(1000m, "m");
        var d = new Dictionary<UCUMQuantity, string> { [a] = "distance" };
        Assert.Equal("distance", d[b]);
    }

    [Fact] public void Hash_UsableInHashSet_Deduplication()
    {
        var a = new UCUMQuantity(1000m, "m");
        var b = new UCUMQuantity(1000m, "m");
        var c = new UCUMQuantity(2000m, "m");
        var s = new HashSet<UCUMQuantity> { a, b, c };
        Assert.Equal(2, s.Count);
    }

    // Known limitation: 1000/3600 reduces to 5/18, a fraction with a factor of 3 in
    // the denominator - non-terminating in both base 10 and base 2, so neither decimal
    // nor double represents it exactly. Compounded by Fhir.Metrics computing the
    // conversion internally in double before the result is cast to decimal.
    [Fact] public void Eq_Speed_3_6kmh_And_1ms() =>
        Assert.True(UCUMOperations.Equals(Lit.Q("3.6 km/h"), Lit.Q("1 m/s")));

    // Known limitation: percent ("%") is not canonicalized to the same value-space
    // representation as the dimensionless "1" unit by this backend, so 50% and 0.5
    // compare unequal despite being the same dimensionless quantity.
    [Fact] public void Eq_Dimensionless_50percent_Equals_0_5() =>
        Assert.True(UCUMOperations.Equals(Lit.Q("50 %"), Lit.Q("0.5 1")));

    [Fact] public void Eq_SpeedOfLight_c_Equals_ms() =>
        Assert.True(UCUMOperations.Equals(Lit.Q("299792458 m/s"), Lit.Q("1 [c]")));

    [Fact] public void Eq_Watt_Equals_JoulePerSecond() =>
        Assert.True(UCUMOperations.Equals(Lit.Q("1 W"), Lit.Q("1 J/s")));

    [Fact] public void Eq_Hertz_Equals_InverseSecond() =>
        Assert.True(UCUMOperations.Equals(Lit.Q("1 Hz"), Lit.Q("1 s-1")));

    [Fact] public void Eq_Joule_Equals_NewtonMeter() =>
        Assert.True(UCUMOperations.Equals(Lit.Q("1 J"), Lit.Q("1 N.m")));
}