using System;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Operators;
using Xunit;

namespace DotNetRdf.Ucum.Tests;

public class Test05_Arithmetic
{
    [Fact] public void Add_SameUnit_CorrectValue() =>
        Assert.Equal(8000m, UCUMQuantity.Parse("5 km").Add(UCUMQuantity.Parse("3 km")).Value);

    [Fact] public void Add_CrossUnit_km_Plus_m_CorrectValue() =>
        Assert.Equal(1500m, UCUMQuantity.Parse("1 km").Add(UCUMQuantity.Parse("500 m")).Value);


    [Fact] public void Add_Time_h_Plus_min_CorrectValue() =>
        Assert.Equal(5400m, UCUMQuantity.Parse("1 h").Add(UCUMQuantity.Parse("30 min")).Value);

    [Fact] public void Add_Newton_Plus_Newton_CorrectValue()
    {
        var r = UCUMQuantity.Parse("1 N").Add(UCUMQuantity.Parse("1 N"));
        Assert.Equal(2000m, r.Value);
    }

    [Fact] public void Add_Force_SameNotation_CorrectValue()
    {
        var r = UCUMQuantity.Parse("10 N").Add(UCUMQuantity.Parse("5 kg.m/s2"));
        Assert.Equal(15000m, r.Value);
    }

    [Fact] public void Add_Energy_1kJ_Plus_500J_CorrectSIValue()
    {
        var r = UCUMQuantity.Parse("1 kJ").Add(UCUMQuantity.Parse("500 J"));
        Assert.Equal(1500000m, r.Value);
    }

    [Fact] public void Add_IncompatibleDimensions_m_Plus_kg_Throws() =>
        Assert.Throws<UCUMDimensionException>(
            () => UCUMQuantity.Parse("1 m").Add(UCUMQuantity.Parse("1 kg")));

    // Known limitation: Fhir.Metrics Axis.Equals ignores the exponent field, so N
    // (kg.m.s-2) is wrongly reported as the same dimension as Pa (kg.m-1.s-2) and J
    // (kg.m2.s-2). Add() relies on SameDimension() to reject incompatible operands,
    // so this addition silently succeeds instead of throwing. Same root cause as the
    // SameDimension_N_And_Pa_False / SameDimension_N_And_J_False failures in
    // Test03_Equality, surfacing here at the arithmetic layer instead of comparison.
    [Fact] public void Add_Newton_Plus_Pascal_Throws() =>
        Assert.Throws<UCUMDimensionException>(
            () => UCUMQuantity.Parse("1 N").Add(UCUMQuantity.Parse("1 Pa")));

    [Fact] public void Add_Newton_Plus_Joule_Throws() =>
        Assert.Throws<UCUMDimensionException>(
            () => UCUMQuantity.Parse("1 N").Add(UCUMQuantity.Parse("1 J")));

    // Known limitation: result unit is always SI base, not original named unit.
    [Fact] public void Add_KmPlusKm_ResultIsNotKm()
    {
        var r = UCUMQuantity.Parse("5 km").Add(UCUMQuantity.Parse("3 km"));
        Assert.NotEqual("km", r.Unit);
        Assert.Equal(8000m, r.Value);
    }

    [Fact] public void Add_NewtonPlusNewton_ResultIsNotN()
    {
        var r = UCUMQuantity.Parse("1 N").Add(UCUMQuantity.Parse("1 N"));
        Assert.NotEqual("N", r.Unit);
    }

    [Fact] public void Subtract_SameUnit_CorrectValue() =>
        Assert.Equal(2000m, UCUMQuantity.Parse("5 km").Subtract(UCUMQuantity.Parse("3 km")).Value);

    [Fact] public void Subtract_CrossUnit_CorrectValue() =>
        Assert.Equal(1500m, UCUMQuantity.Parse("2 km").Subtract(UCUMQuantity.Parse("500 m")).Value);

    [Fact] public void Subtract_ZeroResult() =>
        Assert.Equal(0m, UCUMQuantity.Parse("1 km").Subtract(UCUMQuantity.Parse("1000 m")).Value);

    [Fact] public void Subtract_NegativeResult() =>
        Assert.Equal(-800m, UCUMQuantity.Parse("200 m").Subtract(UCUMQuantity.Parse("1 km")).Value);

    [Fact] public void Subtract_IncompatibleDimensions_Throws() =>
        Assert.Throws<UCUMDimensionException>(
            () => UCUMQuantity.Parse("1 m").Subtract(UCUMQuantity.Parse("1 s")));

    [Fact] public void Multiply_ByInt_CorrectValue() =>
        Assert.Equal(15m, new UCUMQuantity(5m, "g").Multiply(3m).Value);

    [Fact] public void Multiply_ByDecimal_CorrectValue() =>
        Assert.Equal(1m, new UCUMQuantity(2m, "g").Multiply(0.5m).Value);

    [Fact] public void Multiply_ScalarKeepsUnit() =>
        Assert.Equal("g", new UCUMQuantity(5m, "g").Multiply(3m).Unit);

    [Fact] public void Divide_ByInt_CorrectValue() =>
        Assert.Equal(5m, new UCUMQuantity(10m, "g").Divide(2m).Value);

    [Fact] public void Divide_ByDecimal_CorrectValue() =>
        Assert.Equal(2m, new UCUMQuantity(1m, "g").Divide(0.5m).Value);

    [Fact] public void Divide_ScalarKeepsUnit() =>
        Assert.Equal("g", new UCUMQuantity(10m, "g").Divide(2m).Unit);

    [Fact] public void Divide_ByZero_Throws() =>
        Assert.Throws<UCUMArithmeticException>(() => new UCUMQuantity(1m, "m").Divide(0m));

    [Fact] public void Multiply_Length_Times_Length_ResultIsArea()
    {
        var r = UCUMQuantity.Parse("3 m").Multiply(UCUMQuantity.Parse("4 m"));
        Assert.Equal(12m, r.Value);
        Assert.Contains("m2", r.Unit);
    }

    [Fact] public void Divide_Length_By_Time_ResultIsVelocity()
    {
        var r = UCUMQuantity.Parse("100 m").Divide(UCUMQuantity.Parse("10 s"));
        Assert.Equal(10m, r.Value);
        Assert.Equal("m.s-1", r.Unit);
    }

    [Fact] public void Multiply_Mass_Times_Acceleration_IsForce()
    {
        var r = UCUMQuantity.Parse("2 kg").Multiply(UCUMQuantity.Parse("3 m/s2"));
        Assert.Equal(6000m, r.Value);
        Assert.True(UCUMOperations.SameDimension(
            new UCUMQuantityNode(r.ToLexicalForm(), CdtNamespace.UcumUri),
            Lit.Q("1 N")));
    }

    [Fact] public void Divide_Length_By_Length_ResultIsDimensionless()
    {
        var r = UCUMQuantity.Parse("5 m").Divide(UCUMQuantity.Parse("5 m"));
        Assert.Equal(1m, r.Value);
    }

    [Fact] public void Multiply_Voltage_Times_Current_IsPower()
    {
        var r = UCUMQuantity.Parse("10 V").Multiply(UCUMQuantity.Parse("2 A"));
        Assert.True(r.Value > 0);
        Assert.True(UCUMOperations.SameDimension(
            new UCUMQuantityNode(r.ToLexicalForm(), CdtNamespace.UcumUri),
            Lit.Q("1 W")));
    }

    [Fact] public void Divide_Voltage_By_Current_IsResistance()
    {
        var r = UCUMQuantity.Parse("10 V").Divide(UCUMQuantity.Parse("2 A"));
        Assert.True(r.Value > 0);
        Assert.True(UCUMOperations.SameDimension(
            new UCUMQuantityNode(r.ToLexicalForm(), CdtNamespace.UcumUri),
            Lit.Q("1 Ohm")));
    }

    // Known limitation: same Axis.Equals exponent bug as Add_Newton_Plus_Pascal_Throws
    // above, exercised here through the SPARQL-facing CdtAdditionOperator rather than
    // the direct UCUMQuantity.Add API - confirms the bug reaches query evaluation, not
    // just the C# arithmetic methods.
    [Fact] public void AdditionOp_IncompatibleDimensions_ThrowsRdfQueryException() =>
        Assert.Throws<VDS.RDF.Query.RdfQueryException>(
            () => new CdtAdditionOperator().Apply(Lit.Q("1 N"), Lit.Q("1 Pa")));

    [Fact] public void Multiply_Energy_10N_Times_5m_CorrectSIValue()
    {
        var r = UCUMQuantity.Parse("10 N").Multiply(UCUMQuantity.Parse("5 m"));
        Assert.Equal(50000m, r.Value);
        Assert.True(UCUMOperations.SameDimension(
            new UCUMQuantityNode(r.ToLexicalForm(), CdtNamespace.UcumUri), Lit.Q("1 J")));
    }

    [Fact] public void Divide_Power_100J_By_10s_CorrectSIValue()
    {
        var r = UCUMQuantity.Parse("100 J").Divide(UCUMQuantity.Parse("10 s"));
        Assert.Equal(10000m, r.Value);
        Assert.True(UCUMOperations.SameDimension(
            new UCUMQuantityNode(r.ToLexicalForm(), CdtNamespace.UcumUri), Lit.Q("1 W")));
    }

    [Fact] public void Divide_Pressure_10N_By_2m2_CorrectSIValue()
    {
        var r = UCUMQuantity.Parse("10 N").Divide(UCUMQuantity.Parse("2 m2"));
        Assert.Equal(5000m, r.Value);
        Assert.True(UCUMOperations.SameDimension(
            new UCUMQuantityNode(r.ToLexicalForm(), CdtNamespace.UcumUri), Lit.Q("1 Pa")));
    }

    [Fact] public void Divide_Frequency_1_By_0_01s_Equals_100Hz()
    {
        var r = UCUMQuantity.Parse("1 1").Divide(UCUMQuantity.Parse("0.01 s"));
        Assert.Equal(100m, r.Value);
    }

    [Fact] public void Multiply_Chain_KineticEnergy_0_5_times_2kg_times_3ms_squared()
    {
        var m = UCUMQuantity.Parse("2 kg");
        var v = UCUMQuantity.Parse("3 m/s");
        var half = UCUMQuantity.Parse("0.5 1");
        var ke = half.Multiply(m).Multiply(v).Multiply(v);
        Assert.Equal(9000m, ke.Value);
    }




    [Fact] public void Kelvin_Addition_100K_Plus_200K()
    {
        var r = UCUMQuantity.Parse("100 K").Add(UCUMQuantity.Parse("200 K"));
        Assert.Equal(300m, r.Value);
    }

    [Fact] public void Celsius_Addition_Throws()
    {
        Assert.Throws<UCUMUnitException>(() =>
            UCUMQuantity.Parse("10 Cel").Add(UCUMQuantity.Parse("20 Cel")));
    }

    [Fact] public void Dimensionless_Multiply_0_5_By_10m()
    {
        var r = UCUMQuantity.Parse("0.5 1").Multiply(UCUMQuantity.Parse("10 m"));
        Assert.Equal(5m, r.Value);
    }
    [Fact] public void Unit_m_minus2_CanonicalDimension()
    {
        var q = UCUMQuantity.Parse("1 m-2");
        Assert.Equal("m-2", q.Unit);
    }
    [Fact] public void Unit_m_per_s_Equals_m_dot_s_minus1() =>
        Assert.True(UCUMOperations.Equals(Lit.Q("1 m/s"), Lit.Q("1 m.s-1")));

    [Fact] public void UCUMUnit_Cancellation_m_kg_per_kg_Equals_m() =>
        Assert.Equal(new UCUMUnit("m"), new UCUMUnit("m.kg/kg"));

    [Fact] public void UCUMUnit_EquivalentNotation_m_s2_Equals_m_sminus2() =>
        Assert.Equal(new UCUMUnit("m/s2"), new UCUMUnit("m.s-2"));
}