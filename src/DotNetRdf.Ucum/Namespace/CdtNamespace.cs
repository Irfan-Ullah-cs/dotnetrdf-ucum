using System;

namespace DotNetRdf.Ucum;

public static class CdtNamespace
{
    public const string NS = "https://w3id.org/cdt/";

    public static readonly Uri UcumUri = new(NS + "ucum");
    public static readonly Uri UcumUnitUri = new(NS + "ucumunit");
    public static readonly Uri SameDimensionUri = new("https://w3id.org/cdt/sameDimension");

    public static bool IsCdtDatatype(Uri? uri) =>
        uri?.AbsoluteUri.StartsWith(NS, StringComparison.Ordinal) == true;

    public static bool IsCdtQuantityDatatype(Uri? uri) =>
        uri?.AbsoluteUri == UcumUri.AbsoluteUri;

    public static bool IsCdtUnitDatatype(Uri? uri) =>
        uri?.AbsoluteUri == UcumUnitUri.AbsoluteUri;
}
