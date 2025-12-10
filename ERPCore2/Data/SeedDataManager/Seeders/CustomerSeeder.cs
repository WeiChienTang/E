using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Helpers;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 客戶資料種子器
    /// </summary>
    public class CustomerSeeder : IDataSeeder
    {
        public int Order => 20; // 在 CustomerTypeSeeder 之後執行，因為需要 CustomerTypes
        public string Name => "客戶資料";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedCustomersAsync(context);
        }

        /// <summary>
        /// 初始化示例客戶資料
        /// </summary>
        private static async Task SeedCustomersAsync(AppDbContext context)
        {
            if (await context.Customers.AnyAsync())
                return; // 客戶資料已存在

            var customers = new List<Customer>();
            var random = new Random(12345); // 固定種子以保持一致性
            
            // 公司名稱範本
            string[] companyPrefixes = { "台灣", "國際", "全球", "東方", "西方", "南方", "北方", "中央", "永續", "創新", 
                "優質", "精密", "高科技", "智慧", "數位", "雲端", "綠能", "環保", "未來", "先進" };
            string[] companyTypes = { "科技", "機械", "電子", "貿易", "實業", "工業", "資訊", "通訊", "能源", "建設",
                "製造", "材料", "化工", "食品", "紡織", "物流", "運輸", "顧問", "服務", "行銷" };
            string[] companySuffixes = { "股份有限公司", "有限公司", "企業社", "商行", "工作室", "事業有限公司" };
            
            // 聯絡人姓氏
            string[] surnames = { "王", "李", "張", "劉", "陳", "楊", "黃", "趙", "周", "吳",
                "徐", "孫", "馬", "朱", "胡", "郭", "林", "何", "高", "羅",
                "鄭", "梁", "謝", "宋", "唐", "許", "韓", "馮", "鄧", "曹" };
            
            // 聯絡人職稱
            string[] titles = { "經理", "副理", "主任", "課長", "組長", "總監", "協理", "專員", "襄理", "執行長" };
            
            // 客戶狀態
            EntityStatus[] statuses = { EntityStatus.Active, EntityStatus.Active, EntityStatus.Active, EntityStatus.Inactive };

            for (int i = 1; i <= 5; i++)
            {
                var prefix = companyPrefixes[random.Next(companyPrefixes.Length)];
                var type = companyTypes[random.Next(companyTypes.Length)];
                var suffix = companySuffixes[random.Next(companySuffixes.Length)];
                var surname = surnames[random.Next(surnames.Length)];
                var title = titles[random.Next(titles.Length)];
                var status = statuses[random.Next(statuses.Length)];
                
                // 生成8位數稅號
                var taxNumber = (10000000 + i * 11111 % 89999999).ToString().PadLeft(8, '0');
                
                customers.Add(new Customer
                {
                    Code = $"C{i:D3}",
                    CompanyName = $"{prefix}{type}{suffix}",
                    ContactPerson = $"{surname}{title}",
                    TaxNumber = taxNumber,
                    Status = status,
                    CreatedAt = DateTime.Now.AddDays(-random.Next(1, 365)),
                    CreatedBy = "System"
                });
            }

            await context.Customers.AddRangeAsync(customers);
            await context.SaveChangesAsync();
        }
    }
}
