namespace GreenDragonTrading.Domain.Exceptions
{
    public class NotFoundException : DomainException
    {
        public NotFoundException(string entityName, object key)
            : base($"Giá trị {key} không được tìm thấy trong bảng {entityName}.")
        {
        }
    }
}
