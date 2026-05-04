namespace UHPS.API.Auth;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException()
        : base("You don't have permission to access this resource.")
    {
    }

    public ForbiddenAccessException(string message) : base(message)
    {
    }
}
