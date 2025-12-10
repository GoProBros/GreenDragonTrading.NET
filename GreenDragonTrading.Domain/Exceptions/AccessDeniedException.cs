namespace GreenDragonTrading.Domain.Exceptions
{
    public class AccessDeniedException : DomainException
    {
        public AccessDeniedException() { }
        public AccessDeniedException(string message)
            : base(message) { }
        public AccessDeniedException(object user, object resource)
            : base($"Người dùng '{user}' không có quyền truy cập vào '{resource}'.") { }
    }
}
