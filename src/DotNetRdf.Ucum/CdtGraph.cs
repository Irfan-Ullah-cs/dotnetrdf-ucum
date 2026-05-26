using System;
using VDS.RDF;

namespace DotNetRdf.Ucum;

public class CdtGraph : Graph
{
    public override ILiteralNode CreateLiteralNode(string literal, Uri datatype)
    {
        if (CdtNamespace.IsCdtQuantityDatatype(datatype))
            return new UCUMQuantityNode(literal, datatype);
        return base.CreateLiteralNode(literal, datatype);
    }
}
