namespace GreenDragonTrading.Domain.Exceptions;

/// <summary>
/// Exception thrown when there's a conflict (e.g., duplicate entry).
/// </summary>
public class ConflictException : DomainException
{
    public string EntityName { get; }
    public string PropertyName { get; }

    public ConflictException(string entityName, string propertyName, string value)
        : base($"{entityName} with {propertyName} '{value}' already exists.")
    {
        EntityName = entityName;
        PropertyName = propertyName;
    }

    public ConflictException(string message) : base(message)
    {
        EntityName = string.Empty;
        PropertyName = string.Empty;
    }
}
