namespace GreenDragonTrading.Domain.Exceptions;

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object? Key { get; }

    public NotFoundException(string entityName, object? key = null)
        : base($"{entityName} was not found.")
    {
        EntityName = entityName;
        Key = key;
    }

    public NotFoundException(string entityName, object key, string message)
        : base(message)
    {
        EntityName = entityName;
        Key = key;
    }
}
