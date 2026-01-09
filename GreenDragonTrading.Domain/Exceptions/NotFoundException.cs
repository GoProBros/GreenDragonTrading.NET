namespace GreenDragonTrading.Domain.Exceptions
{
    public class NotFoundException : DomainException
    {
        public NotFoundException()
        {
        }
        public NotFoundException(string message)
            : base(message)
        {
        }
        public NotFoundException(string entityName, object key)
            : base($"Giá trị {key} không được tìm thấy trong bảng {entityName}.")
        {
        }
    }
}
