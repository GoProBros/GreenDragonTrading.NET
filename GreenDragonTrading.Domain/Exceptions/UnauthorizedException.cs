namespace GreenDragonTrading.Domain.Exceptions;

/// <summary>
/// Exception thrown when authentication or authorization fails.
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException() : base("Unauthorized access.")
    {
    }

    public UnauthorizedException(string message) : base(message)
    {
    }
}
