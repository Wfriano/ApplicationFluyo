namespace FluyoV2.Exceptions;

public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message)
        : base(message)
    {
    }
}