using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2
{
    /// <summary>
    /// 測試實體 ID 對應的類別
    /// </summary>
    public class TestIdentityMapping
    {
        private readonly AppDbContext _context;

        public TestIdentityMapping(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 測試主實體的 ID 對應
        /// </summary>
        public async Task TestMainEntityMapping()
        {
            // 測試客戶
            var customer = new Customer
            {
                CustomerCode = "TEST001",
                CompanyName = "測試公司",
                Status = EntityStatus.Active
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"Customer Id after save: {customer.Id}");
        }

        /// <summary>
        /// 測試子實體的 ID 對應
        /// </summary>
        public async Task TestSubEntityMapping()
        {
            // 先建立一個客戶
            var customer = new Customer
            {
                CustomerCode = "TEST002", 
                CompanyName = "測試公司2",
                Status = EntityStatus.Active
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // 測試聯絡方式
            var contact = new CustomerContact
            {
                CustomerId = customer.Id,
                ContactValue = "test@example.com",
                Status = EntityStatus.Active
            };

            _context.CustomerContacts.Add(contact);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"CustomerContact Id after save: {contact.Id}");
        }

        /// <summary>
        /// 檢查資料庫欄位對應
        /// </summary>
        public void CheckColumnMapping()
        {
            var customerEntityType = _context.Model.FindEntityType(typeof(Customer));
            var customerIdProperty = customerEntityType?.FindProperty("Id");
            
            var contactEntityType = _context.Model.FindEntityType(typeof(CustomerContact));
            var contactIdProperty = contactEntityType?.FindProperty("Id");

            Console.WriteLine("=== 欄位對應檢查 ===");
            Console.WriteLine($"Customer.Id -> 資料庫欄位: {customerIdProperty?.GetColumnName()}");
            Console.WriteLine($"CustomerContact.Id -> 資料庫欄位: {contactIdProperty?.GetColumnName()}");
            
            // 檢查是否為 Identity
            Console.WriteLine($"Customer.Id IsIdentity: {customerIdProperty?.GetValueGenerationStrategy()}");
            Console.WriteLine($"CustomerContact.Id IsIdentity: {contactIdProperty?.GetValueGenerationStrategy()}");
        }
    }
}
