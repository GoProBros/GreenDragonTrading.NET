namespace GreenDragonTrading.Domain.Exceptions
{
    public class UnauthenticatedException : DomainException
    {
        public UnauthenticatedException(string message)
            : base(message)
        {
        }
        public UnauthenticatedException()
            : base("Người dùng chưa đăng nhập.")
        {
        }
    }
}
