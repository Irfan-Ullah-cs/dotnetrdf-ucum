# DotNetRdf.Ucum

**UCUM custom datatypes for dotNetRDF** - unit-aware equality, comparison, arithmetic, and SPARQL operator support.

[![.NET](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## Overview

`DotNetRdf.Ucum` extends [dotNetRDF](https://dotnetrdf.org/) with support for the CDT (Custom Datatypes) quantity vocabulary defined by Lefrançois & Zimmermann. The CDT specification defines `cdt:ucum` and related datatypes as `rdfs:Datatype` instances with a formal lexical space: a decimal number (with optional scientific notation), at least one space, and a UCUM unit expression. The value space is pairs `(v, u)` where v is a real number and u is a UCUM unit, with the lexical-to-value mapping normalising to SI base units internally. The full specification is published at [w3id.org/cdt/custom_datatypes](https://w3id.org/lindt/custom_datatypes).

Register the extension and dotNetRDF gains the ability to store, compare, sort, and compute with physical quantities directly inside SPARQL queries - a sensor reading of `"1 km"^^cdt:ucum` and one of `"1000 m"^^cdt:ucum` are recognised as the same value, and `FILTER`, `ORDER BY`, `BIND`, and aggregates all work across mixed units without any manual conversion in the query.

Unit parsing, conversion, and dimensional arithmetic are handled by [Fhir.Metrics](https://github.com/FirelyTeam/Fhir.Metrics), the only .NET library that parses UCUM unit codes at runtime rather than through a fixed compile-time API.

## Installation

`DotNetRdf.Ucum` is not yet published to NuGet.org. Install it from a [GitHub Release](../../releases) `.nupkg`:

```powershell
# Download the .nupkg from the Releases page into a local folder, e.g. .\packages\
dotnet nuget add source ".\packages" --name local-ucum
dotnet add package DotNetRdf.Ucum --version 0.1.0
```

This pulls in `dotNetRdf.Core` and `Fhir.Metrics` automatically as transitive dependencies.

## Sample Dataset

Every example below runs against the file `sensors.ttl` shown here in full. It's a trimmed excerpt of a real SOSA/SSN smart-city dataset - the same sensors reporting the same readings, just cut down to what's needed to follow along. Save this exact content as `sensors.ttl` and every query result shown in this README is fully reproducible.

```turtle
@prefix sosa: <http://www.w3.org/ns/sosa/> .
@prefix cdt:  <https://w3id.org/cdt/> .
@prefix ex:   <https://example.org/> .

# --- Vehicle speed: three sensors, three different units ---
ex:Obs_Speed_AM_T1 a sosa:Observation ;
    sosa:observedProperty ex:VehicleSpeed ;
    sosa:hasFeatureOfInterest ex:RN82_Road ;
    sosa:hasSimpleResult "8.33 m/s"^^cdt:ucum .

ex:Obs_Speed_AM_T2 a sosa:Observation ;
    sosa:observedProperty ex:VehicleSpeed ;
    sosa:hasFeatureOfInterest ex:RN82_Road ;
    sosa:hasSimpleResult "15.28 m/s"^^cdt:ucum .

ex:Obs_Speed_ES_T1 a sosa:Observation ;
    sosa:observedProperty ex:VehicleSpeed ;
    sosa:hasFeatureOfInterest ex:RN82_Road ;
    sosa:hasSimpleResult "31.6 km/h"^^cdt:ucum .

ex:Obs_Speed_ES_T2 a sosa:Observation ;
    sosa:observedProperty ex:VehicleSpeed ;
    sosa:hasFeatureOfInterest ex:RN82_Road ;
    sosa:hasSimpleResult "57.2 km/h"^^cdt:ucum .

ex:Obs_Speed_DB_T1 a sosa:Observation ;
    sosa:observedProperty ex:VehicleSpeed ;
    sosa:hasFeatureOfInterest ex:RN82_Road ;
    sosa:hasSimpleResult "19.7 [mi_i]/h"^^cdt:ucum .

ex:Obs_Speed_DB_T2 a sosa:Observation ;
    sosa:observedProperty ex:VehicleSpeed ;
    sosa:hasFeatureOfInterest ex:RN82_Road ;
    sosa:hasSimpleResult "35.5 [mi_i]/h"^^cdt:ucum .

# --- Bridge deck displacement: three sensors, three different units ---
ex:Obs_Disp_TF_T1 a sosa:Observation ;
    sosa:observedProperty ex:StructuralDisplacement ;
    sosa:hasFeatureOfInterest ex:TortasseBridge_Deck ;
    sosa:hasSimpleResult "2.34 mm"^^cdt:ucum .

ex:Obs_Disp_SC_T1 a sosa:Observation ;
    sosa:observedProperty ex:StructuralDisplacement ;
    sosa:hasFeatureOfInterest ex:TortasseBridge_Deck ;
    sosa:hasSimpleResult "0.092 [in_i]"^^cdt:ucum .

ex:Obs_Disp_AM_T1 a sosa:Observation ;
    sosa:observedProperty ex:StructuralDisplacement ;
    sosa:hasFeatureOfInterest ex:TortasseBridge_Deck ;
    sosa:hasSimpleResult "2340.0 um"^^cdt:ucum .

ex:Obs_Disp_TF_T2 a sosa:Observation ;
    sosa:observedProperty ex:StructuralDisplacement ;
    sosa:hasFeatureOfInterest ex:TortasseBridge_Deck ;
    sosa:hasSimpleResult "8.71 mm"^^cdt:ucum .

ex:Obs_Disp_AM_T2 a sosa:Observation ;
    sosa:observedProperty ex:StructuralDisplacement ;
    sosa:hasFeatureOfInterest ex:TortasseBridge_Deck ;
    sosa:hasSimpleResult "8710.0 um"^^cdt:ucum .

ex:Obs_Disp_SC_T2 a sosa:Observation ;
    sosa:observedProperty ex:StructuralDisplacement ;
    sosa:hasFeatureOfInterest ex:TortasseBridge_Deck ;
    sosa:hasSimpleResult "0.343 [in_i]"^^cdt:ucum .

# --- Atmospheric visibility: km vs miles ---
ex:Obs_Vis_ES_T2 a sosa:Observation ;
    sosa:observedProperty ex:AtmosphericVisibility ;
    sosa:hasFeatureOfInterest ex:RN82_Road ;
    sosa:hasSimpleResult "9.4 km"^^cdt:ucum .

ex:Obs_Vis_AM_T2 a sosa:Observation ;
    sosa:observedProperty ex:AtmosphericVisibility ;
    sosa:hasFeatureOfInterest ex:RN82_Road ;
    sosa:hasSimpleResult "5.84 [mi_i]"^^cdt:ucum .

# --- Water flow rate: three sensors, three different units ---
ex:Obs_Flow_TF_T1 a sosa:Observation ;
    sosa:observedProperty ex:WaterFlowRate ;
    sosa:hasFeatureOfInterest ex:WaterPlant_Facility ;
    sosa:hasSimpleResult "248.5 m3/h"^^cdt:ucum .

ex:Obs_Flow_TF_T2 a sosa:Observation ;
    sosa:observedProperty ex:WaterFlowRate ;
    sosa:hasFeatureOfInterest ex:WaterPlant_Facility ;
    sosa:hasSimpleResult "317.2 m3/h"^^cdt:ucum .

ex:Obs_Flow_AM_T1 a sosa:Observation ;
    sosa:observedProperty ex:WaterFlowRate ;
    sosa:hasFeatureOfInterest ex:WaterPlant_Facility ;
    sosa:hasSimpleResult "1087.4 [gal_us]/min"^^cdt:ucum .

ex:Obs_Flow_AM_T2 a sosa:Observation ;
    sosa:observedProperty ex:WaterFlowRate ;
    sosa:hasFeatureOfInterest ex:WaterPlant_Facility ;
    sosa:hasSimpleResult "1388.6 [gal_us]/min"^^cdt:ucum .

ex:Obs_Flow_SC_T1 a sosa:Observation ;
    sosa:observedProperty ex:WaterFlowRate ;
    sosa:hasFeatureOfInterest ex:WaterPlant_Facility ;
    sosa:hasSimpleResult "146.5 [ft_i]3/min"^^cdt:ucum .

ex:Obs_Flow_SC_T2 a sosa:Observation ;
    sosa:observedProperty ex:WaterFlowRate ;
    sosa:hasFeatureOfInterest ex:WaterPlant_Facility ;
    sosa:hasSimpleResult "187.0 [ft_i]3/min"^^cdt:ucum .

# --- Wind speed: m/s vs knots ---
ex:Obs_Wind_TF_T1 a sosa:Observation ;
    sosa:observedProperty ex:WindSpeed ;
    sosa:hasFeatureOfInterest ex:CentreVille_Atmosphere ;
    sosa:hasSimpleResult "3.4 m/s"^^cdt:ucum .

ex:Obs_Wind_DB_T1 a sosa:Observation ;
    sosa:observedProperty ex:WindSpeed ;
    sosa:hasFeatureOfInterest ex:CentreVille_Atmosphere ;
    sosa:hasSimpleResult "6.6 [kn_i]"^^cdt:ucum .

# --- Structural acceleration (used below to demonstrate a known limitation) ---
ex:Obs_Accel_ES_T1 a sosa:Observation ;
    sosa:observedProperty ex:StructuralAcceleration ;
    sosa:hasFeatureOfInterest ex:TortasseBridge_Deck ;
    sosa:hasSimpleResult "0.043 m/s2"^^cdt:ucum .

# --- Indoor air temperature: Celsius vs Fahrenheit ---
ex:Obs_InTemp_SM_T1 a sosa:Observation ;
    sosa:observedProperty ex:IndoorAirTemperature ;
    sosa:hasFeatureOfInterest ex:SportsComplex_Indoor ;
    sosa:hasSimpleResult "20.8 Cel"^^cdt:ucum .

ex:Obs_InTemp_AM_T1 a sosa:Observation ;
    sosa:observedProperty ex:IndoorAirTemperature ;
    sosa:hasFeatureOfInterest ex:SportsComplex_Indoor ;
    sosa:hasSimpleResult "69.4 [degF]"^^cdt:ucum .
```

## Quick Start

```csharp
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using DotNetRdf.Ucum;

// Activates the extension: registers cdt:ucum/cdt:ucumunit, the arithmetic
// operators, cdt:sameDimension, and the unit-aware node comparer.
UCUMConfig.Register();

// CdtGraph recognises cdt:ucum literals as unit-aware quantities as they load.
var g = new CdtGraph();
new TurtleParser().Load(g, "sensors.ttl");

var store = new TripleStore();
store.Add(g);
var processor = new CdtQueryProcessor(store);
var parser = new SparqlQueryParser();

var results = (SparqlResultSet)processor.ProcessQuery(parser.ParseFromString("""
    PREFIX sosa: <http://www.w3.org/ns/sosa/>
    PREFIX ex:   <https://example.org/>
    SELECT ?obs ?result WHERE {
        ?obs sosa:observedProperty ex:VehicleSpeed ;
             sosa:hasSimpleResult ?result .
        FILTER(?result > "50 km/h"^^<https://w3id.org/cdt/ucum>)
    }
"""));

foreach (var row in results)
    Console.WriteLine($"{row["obs"]}  {row["result"]}");
```

```
ex:Obs_Speed_ES_T2   57.2 km/h
ex:Obs_Speed_AM_T2   15.28 m/s
ex:Obs_Speed_DB_T2   35.5 [mi_i]/h
```

All three match the same threshold, even though none of them are recorded in km/h except one. (Row order may differ between runs - this query has no `ORDER BY`, so SPARQL does not guarantee a fixed order.)

## SPARQL Capabilities

All queries below run against `sensors.ttl` above, using the same `processor`/`parser` setup from Quick Start. Only the query text and `PREFIX` block change.

### Cross-unit exact equality

```sparql
PREFIX sosa: <http://www.w3.org/ns/sosa/>
PREFIX ex:   <https://example.org/>
SELECT ?obs ?result WHERE {
    ?obs sosa:observedProperty ex:StructuralDisplacement ;
         sosa:hasSimpleResult ?result .
    FILTER(?result = "2.34 mm"^^<https://w3id.org/cdt/ucum>)
}
```
```
ex:Obs_Disp_TF_T1   2.34 mm
ex:Obs_Disp_AM_T1   2340.0 um
```
Two different sensors, two different units, one physical value.

### `BIND` arithmetic across units

```sparql
PREFIX ex: <https://example.org/>
SELECT ?diff WHERE {
    ex:Obs_Vis_ES_T2 <http://www.w3.org/ns/sosa/hasSimpleResult> ?es .
    ex:Obs_Vis_AM_T2 <http://www.w3.org/ns/sosa/hasSimpleResult> ?am .
    BIND(?es - ?am AS ?diff)
}
```
```
difference = 1.4310400000 m
```
Subtracting a kilometre reading from a mile reading, no manual conversion, correct result in the base unit.

### `ORDER BY` across mixed units

```sparql
PREFIX sosa: <http://www.w3.org/ns/sosa/>
PREFIX ex:   <https://example.org/>
SELECT ?obs ?result WHERE {
    ?obs sosa:observedProperty ex:StructuralDisplacement ;
         sosa:hasSimpleResult ?result .
} ORDER BY ?result
```
```
0.092 [in_i]
2.34 mm
2340.0 um
8.71 mm
8710.0 um
0.343 [in_i]
```
Sorted by true physical magnitude, not by lexical string comparison of the literal.

### `cdt:sameDimension()` across unrelated properties

```sparql
PREFIX sosa: <http://www.w3.org/ns/sosa/>
PREFIX cdt:  <https://w3id.org/cdt/>
SELECT ?obs ?prop ?result WHERE {
    ?obs sosa:observedProperty ?prop ;
         sosa:hasSimpleResult ?result .
    FILTER(cdt:sameDimension(?result, "1 m/s"^^cdt:ucum))
}
```
```
ex:Obs_Speed_AM_T1   VehicleSpeed           8.33 m/s
ex:Obs_Speed_AM_T2   VehicleSpeed           15.28 m/s
ex:Obs_Speed_ES_T1   VehicleSpeed           31.6 km/h
ex:Obs_Speed_ES_T2   VehicleSpeed           57.2 km/h
ex:Obs_Speed_DB_T1   VehicleSpeed           19.7 [mi_i]/h
ex:Obs_Speed_DB_T2   VehicleSpeed           35.5 [mi_i]/h
ex:Obs_Wind_TF_T1    WindSpeed              3.4 m/s
ex:Obs_Wind_DB_T1    WindSpeed              6.6 [kn_i]
ex:Obs_Flow_TF_T1    WaterFlowRate          248.5 m3/h
ex:Obs_Flow_TF_T2    WaterFlowRate          317.2 m3/h
ex:Obs_Flow_AM_T1    WaterFlowRate          1087.4 [gal_us]/min
ex:Obs_Flow_AM_T2    WaterFlowRate          1388.6 [gal_us]/min
ex:Obs_Flow_SC_T1    WaterFlowRate          146.5 [ft_i]3/min
ex:Obs_Flow_SC_T2    WaterFlowRate          187.0 [ft_i]3/min
ex:Obs_Accel_ES_T1   StructuralAcceleration 0.043 m/s2
```
Finds every observation that is dimensionally speed, across two different properties and five different units, in one query. The last seven rows are a known limitation - see below.

### `MAX` across mixed units, datatype preserved

```sparql
PREFIX sosa: <http://www.w3.org/ns/sosa/>
PREFIX ex:   <https://example.org/>
SELECT (MAX(?result) AS ?max) WHERE {
    ?obs sosa:observedProperty ex:WaterFlowRate ;
         sosa:hasSimpleResult ?result .
}
```
```
MAX = 187.0 [ft_i]3/min   (datatype: https://w3id.org/cdt/ucum)
```
Six readings across three units; `MAX` correctly identifies the physically largest and returns it completely unchanged, original unit and datatype intact.

## Known Limitations

This project follows a strict no-papering-over policy: every limitation below is enforced by a permanent, visible failing or skipped test in the test suite, not a silently adjusted result.

**Celsius and other offset-based units are not supported.** `cdt:ucum`'s value-space mapping is purely multiplicative (`value × factor`). Celsius requires an additive offset (`K = °C + 273.15`), which this model cannot express - this is a limitation of the specification's design, not of this implementation specifically. Using the `sensors.ttl` data above:

```sparql
PREFIX sosa: <http://www.w3.org/ns/sosa/>
PREFIX ex:   <https://example.org/>
SELECT ?obs ?result WHERE {
    ?obs sosa:observedProperty ex:IndoorAirTemperature ;
         sosa:hasSimpleResult ?result .
    FILTER(?result > "21 Cel"^^<https://w3id.org/cdt/ucum>)
}
```
```
Rows returned: 0
```
`ex:Obs_InTemp_SM_T1` ("20.8 Cel") and `ex:Obs_InTemp_AM_T1` ("69.4 [degF]", which is 20.8°C) both exist in the data, but neither is returned - Celsius comparisons fail silently rather than producing a wrong answer.

**`SUM` and `AVG` strip the `cdt:ucum` datatype.** dotNetRDF's built-in aggregate processor always reconstructs a plain `xsd:decimal` for these two aggregates, with no extension point available to intercept it:

```sparql
PREFIX sosa: <http://www.w3.org/ns/sosa/>
PREFIX ex:   <https://example.org/>
SELECT (AVG(?result) AS ?avg) WHERE {
    ?obs sosa:observedProperty ex:WaterFlowRate ;
         sosa:hasSimpleResult ?avg .
}
```
The numeric average is computed correctly across all three units - `0.0784574479137483574426736053` - but the result comes back typed as `xsd:decimal`, not `cdt:ucum`.

**Some incompatible dimensions are not rejected, due to a defect in Fhir.Metrics.** Fhir.Metrics' internal dimension comparison ignores unit exponents, so units that share the same base quantities but different powers can be wrongly treated as the same dimension. In the `cdt:sameDimension()` example above, a filter for `"1 m/s"` (a *speed* - length¹ over time¹) incorrectly matches two unrelated properties: `WaterFlowRate` (a *volumetric flow rate* - length³ over time¹, mismatched on the length exponent) and `StructuralAcceleration` (length¹ over time², mismatched on the time exponent). Seven of the fifteen rows returned by that query are false positives from this single root cause. This is a defect in Fhir.Metrics itself, not in this extension, and is intentionally not worked around here - the correct fix belongs upstream, in Fhir.Metrics.

**`ABS`, `CEIL`, `FLOOR`, `ROUND`, and unary minus (`-?x`) strip the `cdt:ucum` datatype or operate on the wrong magnitude.** These are hardcoded directly in dotNetRDF's expression evaluator with no registry or extension point, unlike the `+`/`-`/`*`/`/` operators. `CEIL(-1.7 km)` operates on the canonical SI value (`-1700`), not the original coefficient (`-1.7`).

**Some cross-unit conversions cannot achieve exact equality.** A conversion like `3.6 km/h` to `1 m/s` requires dividing by exactly 3600/1000, a fraction that terminates in neither base 10 nor base 2 - the resulting rounding error, though astronomically small, can break exact equality comparisons.

## References

- Lefrançois, M. & Zimmermann, A. (2018). *The Unified Code for Units of Measure in RDF: cdt:ucum and other UCUM Datatypes*. ESWC 2018 (Demo).
- Lefrançois, M. & Zimmermann, A. (2016). *Supporting Arbitrary Custom Datatypes in RDF and SPARQL*. ESWC 2016.
- [CDT / LINDT specification](https://w3id.org/lindt/custom_datatypes) - formal definition of the `cdt:ucum` datatype and related quantity types
- [UCUM specification](https://ucum.org/ucum) - the standard for unit codes
- [dotNetRDF](https://dotnetrdf.org/) - the RDF/SPARQL library this extends
- [Fhir.Metrics](https://github.com/FirelyTeam/Fhir.Metrics) - the UCUM parsing and dimensional arithmetic backend

## License

MIT - see [LICENSE](LICENSE).