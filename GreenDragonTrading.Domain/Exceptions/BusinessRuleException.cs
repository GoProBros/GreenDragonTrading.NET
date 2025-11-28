namespace GreenDragonTrading.Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : DomainException
{
    public string Code { get; }

    public BusinessRuleException(string message) : base(message)
    {
        Code = "BUSINESS_RULE_VIOLATION";
    }

    public BusinessRuleException(string code, string message) : base(message)
    {
        Code = code;
    }
}
