using System;

namespace DotNetRdf.Ucum.Exceptions;

public class UCUMException : Exception
{
    public UCUMException(string message) : base(message) { }
    public UCUMException(string message, Exception inner) : base(message, inner) { }
}

// Lexical form could not be parsed as a CDT quantity or unit literal.
public class UCUMParseException : UCUMException
{
    public UCUMParseException(string message) : base(message) { }
    public UCUMParseException(string message, Exception inner) : base(message, inner) { }
}

// The UCUM unit code is not recognised by the unit system.
public class UCUMUnitException : UCUMException
{
    public UCUMUnitException(string message) : base(message) { }
    public UCUMUnitException(string message, Exception inner) : base(message, inner) { }
}

// Operation requires commensurable dimensions but the operands have incompatible dimensions.
public class UCUMDimensionException : UCUMException
{
    public UCUMDimensionException(string message) : base(message) { }
    public UCUMDimensionException(string message, Exception inner) : base(message, inner) { }
}

// Arithmetic error such as division by zero.
public class UCUMArithmeticException : UCUMException
{
    public UCUMArithmeticException(string message) : base(message) { }
    public UCUMArithmeticException(string message, Exception inner) : base(message, inner) { }
}
