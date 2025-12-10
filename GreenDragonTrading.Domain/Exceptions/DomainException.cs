namespace GreenDragonTrading.Domain.Exceptions
{
    public abstract class DomainException : Exception
    {
        public DomainException()
        : base() { }

        public DomainException(string message)
            : base(message) { }

        public DomainException(string message, Exception inner)
            : base(message, inner) { }
    }
}
