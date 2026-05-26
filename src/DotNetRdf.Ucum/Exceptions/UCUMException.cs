using System;

namespace DotNetRdf.Ucum.Exceptions;

public class UCUMException : Exception
{
    public UCUMException(string message) : base(message) { }
    public UCUMException(string message, Exception inner) : base(message, inner) { }
}

public class UCUMParseException : UCUMException
{
    public UCUMParseException(string message) : base(message) { }
    public UCUMParseException(string message, Exception inner) : base(message, inner) { }
}

public class UCUMUnitException : UCUMException
{
    public UCUMUnitException(string message) : base(message) { }
    public UCUMUnitException(string message, Exception inner) : base(message, inner) { }
}

public class UCUMDimensionException : UCUMException
{
    public UCUMDimensionException(string message) : base(message) { }
    public UCUMDimensionException(string message, Exception inner) : base(message, inner) { }
}

public class UCUMArithmeticException : UCUMException
{
    public UCUMArithmeticException(string message) : base(message) { }
    public UCUMArithmeticException(string message, Exception inner) : base(message, inner) { }
}