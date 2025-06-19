using ERPCore2.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Test;

public class DatabaseTestHelper
{
    public static async Task TestDatabaseConnection()
    {
        var connectionString = "Server=localhost\\SQLEXPRESS;Database=ERPCore2;User ID=sa;Password=hdcomp1106;TrustServerCertificate=true;MultipleActiveResultSets=true";
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using var context = new AppDbContext(options);
        
        Console.WriteLine("測試資料庫連接...");
        
        try
        {
            // 測試連接
            await context.Database.CanConnectAsync();
            Console.WriteLine("✓ 資料庫連接成功");

            // 測試查詢客戶
            var customers = await context.Customers.Take(5).ToListAsync();
            Console.WriteLine($"✓ 查詢到 {customers.Count} 筆客戶資料:");
            
            foreach (var customer in customers)
            {
                Console.WriteLine($"  - ID: {customer.Id}, 代碼: {customer.CustomerCode}, 公司: {customer.CompanyName}");
            }

            // 測試子集合查詢
            var customerWithDetails = await context.Customers
                .Include(c => c.CustomerContacts)
                .Include(c => c.CustomerAddresses)
                .FirstOrDefaultAsync();
                
            if (customerWithDetails != null)
            {
                Console.WriteLine($"✓ 客戶 {customerWithDetails.CompanyName} 的子集合:");
                Console.WriteLine($"  - 聯絡方式數量: {customerWithDetails.CustomerContacts?.Count ?? 0}");
                Console.WriteLine($"  - 地址數量: {customerWithDetails.CustomerAddresses?.Count ?? 0}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 錯誤: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"內部錯誤: {ex.InnerException.Message}");
            }
        }
    }
}
