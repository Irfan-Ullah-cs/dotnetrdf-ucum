using System;
using Xunit;

namespace DotNetRdf.Ucum.Tests;

public class Test02_Namespace
{
    [Fact] public void UcumUri_CorrectValue() =>
        Assert.Equal("https://w3id.org/cdt/ucum", CdtNamespace.UcumUri.AbsoluteUri);

    [Fact] public void UcumUnitUri_CorrectValue() =>
        Assert.Equal("https://w3id.org/cdt/ucumunit", CdtNamespace.UcumUnitUri.AbsoluteUri);
}
