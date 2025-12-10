using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenDragonTrading.Domain.Exceptions
{
    public class ConflictException : DomainException
    {
        public ConflictException()
        {
        }
        public ConflictException(string message)
            : base(message)
        {
        }
        public ConflictException(string message, Exception inner)
            : base(message, inner)
        {
        }
        public ConflictException(string entityName, string conflictField, string conflictValue)
        : base($"Lỗi xung đột. Bảng {entityName} với cột {conflictField} đã tồn tại giá trị '{conflictValue}'.")
        {
        }
    }
}
