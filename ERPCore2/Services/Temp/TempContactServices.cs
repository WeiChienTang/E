// 這個檔案暫時用來避免編譯錯誤
// 待移除舊的 Contact 服務後刪除

using ERPCore2.Data.Entities;

namespace ERPCore2.Services
{
    // 暫時的空介面，避免 ServiceRegistration 編譯錯誤
    public interface ICustomerContactService { }
    public interface ISupplierContactService { }
    public interface IEmployeeContactService { }
    
    // 暫時的空服務，避免 ServiceRegistration 編譯錯誤
    public class CustomerContactService : ICustomerContactService { }
    public class SupplierContactService : ISupplierContactService { }
    public class EmployeeContactService : IEmployeeContactService { }
}
